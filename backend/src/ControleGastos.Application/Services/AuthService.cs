using ControleGastos.Application.Auth;
using ControleGastos.Application.DTOs;
using ControleGastos.Application.Errors;
using ControleGastos.Application.Exceptions;
using ControleGastos.Application.Interfaces;
using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Interfaces;

namespace ControleGastos.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPersonRepository personRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator accessTokenGenerator,
    IRefreshTokenFactory refreshTokenFactory,
    IGoogleTokenValidator googleTokenValidator,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<AuthSessionResult> RegisterAsync(AuthRegisterDto dto)
    {
        var normalizedEmail = NormalizeEmail(dto.Email);
        var existingUser = await userRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser is not null)
            throw new BusinessRuleException(BusinessErrorMessages.EmailAlreadyInUse);

        var nowUtc = dateTimeProvider.UtcNow;
        var person = await personRepository.CreateAsync(new Person(dto.Name, nowUtc.Date.AddYears(-18)));

        var user = User.CreateWithPassword(
            normalizedEmail,
            dto.Name,
            passwordHasher.Hash(dto.Password),
            person.Id,
            nowUtc);

        await userRepository.CreateAsync(user);
        return await CreateSessionAsync(user);
    }

    public async Task<AuthSessionResult> LoginAsync(AuthLoginDto dto)
    {
        var normalizedEmail = NormalizeEmail(dto.Email);
        var user = await userRepository.GetByEmailAsync(normalizedEmail);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash) || !passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new BusinessRuleException(BusinessErrorMessages.InvalidCredentials);

        return await CreateSessionAsync(user);
    }

    public async Task<AuthSessionResult> LoginWithGoogleAsync(GoogleAuthDto dto, CancellationToken cancellationToken = default)
    {
        var payload = await googleTokenValidator.ValidateAsync(dto.IdToken, cancellationToken);
        if (payload is null)
            throw new BusinessRuleException(BusinessErrorMessages.InvalidGoogleToken);

        if (!payload.EmailVerified)
            throw new BusinessRuleException(BusinessErrorMessages.GoogleEmailNotVerified);

        var normalizedEmail = NormalizeEmail(payload.Email);
        var googleDisplayName = string.IsNullOrWhiteSpace(payload.Name)
            ? normalizedEmail.Split('@')[0]
            : payload.Name;
        var nowUtc = dateTimeProvider.UtcNow;

        var existingBySub = await userRepository.GetByGoogleSubAsync(payload.Sub);
        if (existingBySub is not null)
            return await CreateSessionAsync(existingBySub);

        var existingByEmail = await userRepository.GetByEmailAsync(normalizedEmail);
        if (existingByEmail is not null)
        {
            existingByEmail.LinkGoogle(payload.Sub, nowUtc);

            existingByEmail.UpdateName(googleDisplayName, nowUtc);

            await userRepository.UpdateAsync(existingByEmail);
            return await CreateSessionAsync(existingByEmail);
        }

        var person = await personRepository.CreateAsync(new Person(googleDisplayName, nowUtc.Date.AddYears(-18)));

        var user = User.CreateWithGoogle(
            normalizedEmail,
            googleDisplayName,
            payload.Sub,
            person.Id,
            nowUtc);

        await userRepository.CreateAsync(user);
        return await CreateSessionAsync(user);
    }

    public async Task<AuthSessionResult> RefreshAsync(string refreshTokenRaw)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenRaw))
            throw new BusinessRuleException(BusinessErrorMessages.InvalidRefreshToken);

        var nowUtc = dateTimeProvider.UtcNow;
        var refreshTokenHash = refreshTokenFactory.Hash(refreshTokenRaw);
        var tokenEntity = await refreshTokenRepository.GetByHashAsync(refreshTokenHash);

        if (tokenEntity is null || !tokenEntity.IsActive(nowUtc))
            throw new BusinessRuleException(BusinessErrorMessages.InvalidRefreshToken);

        tokenEntity.Revoke(nowUtc);
        await refreshTokenRepository.UpdateAsync(tokenEntity);

        var user = tokenEntity.User ?? await userRepository.GetByIdAsync(tokenEntity.UserId);
        if (user is null)
            throw new BusinessRuleException(BusinessErrorMessages.InvalidRefreshToken);

        return await CreateSessionAsync(user);
    }

    public async Task LogoutAsync(string refreshTokenRaw)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenRaw))
            return;

        var tokenHash = refreshTokenFactory.Hash(refreshTokenRaw);
        var tokenEntity = await refreshTokenRepository.GetByHashAsync(tokenHash);
        if (tokenEntity is null)
            return;

        tokenEntity.Revoke(dateTimeProvider.UtcNow);
        await refreshTokenRepository.UpdateAsync(tokenEntity);
    }

    private async Task<AuthSessionResult> CreateSessionAsync(User user)
    {
        var nowUtc = dateTimeProvider.UtcNow;
        var accessToken = accessTokenGenerator.Generate(user);
        var refreshTokenGenerated = refreshTokenFactory.Generate();

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenGenerated.Hash,
            refreshTokenGenerated.ExpiresAtUtc,
            nowUtc);

        await refreshTokenRepository.CreateAsync(refreshToken);

        var expiresIn = Math.Max(1, (int)Math.Ceiling((accessToken.ExpiresAtUtc - nowUtc).TotalSeconds));

        return new AuthSessionResult
        {
            RefreshToken = refreshTokenGenerated.RawToken,
            RefreshTokenExpiresAtUtc = refreshTokenGenerated.ExpiresAtUtc,
            Response = new AuthResponseDto
            {
                AccessToken = accessToken.Token,
                ExpiresIn = expiresIn,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    AuthProvider = user.AuthProvider,
                    PersonId = user.PersonId
                }
            }
        };
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleException(BusinessErrorMessages.InvalidCredentials);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        return normalizedEmail.Contains('@')
            ? normalizedEmail
            : throw new BusinessRuleException(BusinessErrorMessages.InvalidCredentials);
    }
}
