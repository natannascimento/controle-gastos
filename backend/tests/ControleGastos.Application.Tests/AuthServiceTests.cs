using ControleGastos.Application.Auth;
using ControleGastos.Application.DTOs;
using ControleGastos.Application.Errors;
using ControleGastos.Application.Exceptions;
using ControleGastos.Application.Interfaces;
using ControleGastos.Application.Services;
using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Interfaces;
using Xunit;

namespace ControleGastos.Application.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnSession()
    {
        var now = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc);
        var fixture = new AuthServiceFixture(now);

        var result = await fixture.Service.RegisterAsync(new AuthRegisterDto
        {
            Email = "User@Test.com",
            Name = "Alice",
            Password = "12345678"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Response.AccessToken));
        Assert.Equal("user@test.com", result.Response.User.Email);
        Assert.NotNull(result.Response.User.PersonId);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenCredentialsAreInvalid()
    {
        var fixture = new AuthServiceFixture(DateTime.UtcNow);
        await fixture.Service.RegisterAsync(new AuthRegisterDto
        {
            Email = "user@test.com",
            Name = "Alice",
            Password = "12345678"
        });

        var act = () => fixture.Service.LoginAsync(new AuthLoginDto
        {
            Email = "user@test.com",
            Password = "wrong-pass"
        });

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(act);
        Assert.Equal(BusinessErrorMessages.InvalidCredentials, exception.Message);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRotateRefreshToken()
    {
        var now = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc);
        var fixture = new AuthServiceFixture(now);

        var firstSession = await fixture.Service.RegisterAsync(new AuthRegisterDto
        {
            Email = "user@test.com",
            Name = "Alice",
            Password = "12345678"
        });

        var refreshed = await fixture.Service.RefreshAsync(firstSession.RefreshToken);

        Assert.NotEqual(firstSession.RefreshToken, refreshed.RefreshToken);

        var refreshAgainAct = () => fixture.Service.RefreshAsync(firstSession.RefreshToken);
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(refreshAgainAct);
        Assert.Equal(BusinessErrorMessages.InvalidRefreshToken, exception.Message);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ShouldLinkAccount_WhenEmailAlreadyExists()
    {
        var fixture = new AuthServiceFixture(DateTime.UtcNow);

        var initialSession = await fixture.Service.RegisterAsync(new AuthRegisterDto
        {
            Email = "link@test.com",
            Name = "Alice",
            Password = "12345678"
        });

        var googleSession = await fixture.Service.LoginWithGoogleAsync(new GoogleAuthDto
        {
            IdToken = "token-link"
        });

        Assert.Equal(initialSession.Response.User.Id, googleSession.Response.User.Id);
        Assert.Equal("Google", googleSession.Response.User.AuthProvider.ToString());
    }

    private sealed class AuthServiceFixture
    {
        public AuthService Service { get; }

        public AuthServiceFixture(DateTime now)
        {
            var userRepo = new InMemoryUserRepository();
            var personRepo = new InMemoryPersonRepository();
            var refreshTokenRepo = new InMemoryRefreshTokenRepository(userRepo);

            Service = new AuthService(
                userRepo,
                refreshTokenRepo,
                personRepo,
                new FakePasswordHasher(),
                new FakeAccessTokenGenerator(now),
                new FakeRefreshTokenFactory(now),
                new FakeGoogleTokenValidator(),
                new FakeDateTimeProvider(now));
        }
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];

        public Task<User> CreateAsync(User user)
        {
            _users.Add(user);
            return Task.FromResult(user);
        }

        public Task<User?> GetByIdAsync(Guid id)
            => Task.FromResult(_users.FirstOrDefault(x => x.Id == id));

        public Task<User?> GetByEmailAsync(string email)
            => Task.FromResult(_users.FirstOrDefault(x => x.Email == email.Trim().ToLowerInvariant()));

        public Task<User?> GetByGoogleSubAsync(string googleSub)
            => Task.FromResult(_users.FirstOrDefault(x => x.GoogleSub == googleSub));

        public Task<User> UpdateAsync(User user)
            => Task.FromResult(user);
    }

    private sealed class InMemoryPersonRepository : IPersonRepository
    {
        private readonly List<Person> _people = [];

        public Task<Person> CreateAsync(Person person)
        {
            _people.Add(person);
            return Task.FromResult(person);
        }

        public Task<IEnumerable<Person>> GetAllAsync()
            => Task.FromResult(_people.AsEnumerable());

        public Task<Person?> GetByIdAsync(Guid id)
            => Task.FromResult(_people.FirstOrDefault(x => x.Id == id));

        public Task<Person> UpdateAsync(Person person)
            => Task.FromResult(person);

        public Task DeleteAsync(Person person)
        {
            _people.Remove(person);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryRefreshTokenRepository(InMemoryUserRepository userRepository) : IRefreshTokenRepository
    {
        private readonly List<RefreshToken> _tokens = [];

        public Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            _tokens.Add(refreshToken);
            return Task.FromResult(refreshToken);
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
        {
            var token = _tokens.FirstOrDefault(x => x.TokenHash == tokenHash);
            if (token is null)
                return null;

            return await userRepository.GetByIdAsync(token.UserId) is null ? null : token;
        }

        public Task RevokeAllByUserAsync(Guid userId, DateTime nowUtc)
        {
            foreach (var token in _tokens.Where(x => x.UserId == userId && x.RevokedAt is null))
                token.Revoke(nowUtc);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(RefreshToken refreshToken)
            => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hash::{password}";
        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }

    private sealed class FakeAccessTokenGenerator(DateTime now) : IAccessTokenGenerator
    {
        public AccessTokenResult Generate(User user)
        {
            return new AccessTokenResult
            {
                Token = $"token-{user.Id}",
                ExpiresAtUtc = now.AddMinutes(15)
            };
        }
    }

    private sealed class FakeRefreshTokenFactory(DateTime now) : IRefreshTokenFactory
    {
        private int _counter;

        public GeneratedRefreshToken Generate()
        {
            _counter++;
            var raw = $"raw-{_counter}";

            return new GeneratedRefreshToken
            {
                RawToken = raw,
                Hash = Hash(raw),
                ExpiresAtUtc = now.AddDays(7)
            };
        }

        public string Hash(string rawToken) => $"hash::{rawToken}";
    }

    private sealed class FakeGoogleTokenValidator : IGoogleTokenValidator
    {
        public Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
        {
            if (idToken == "token-link")
            {
                return Task.FromResult<GoogleTokenPayload?>(new GoogleTokenPayload
                {
                    Sub = "google-sub",
                    Email = "link@test.com",
                    Name = "Alice Updated",
                    EmailVerified = true
                });
            }

            return Task.FromResult<GoogleTokenPayload?>(null);
        }
    }

    private sealed class FakeDateTimeProvider(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }
}
