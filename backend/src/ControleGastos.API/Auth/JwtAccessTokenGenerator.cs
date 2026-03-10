using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ControleGastos.Application.Auth;
using ControleGastos.Application.Interfaces;
using ControleGastos.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ControleGastos.API.Auth;

public class JwtAccessTokenGenerator(IOptions<JwtOptions> jwtOptions, IDateTimeProvider dateTimeProvider) : IAccessTokenGenerator
{
    public AccessTokenResult Generate(User user)
    {
        var options = jwtOptions.Value;
        var nowUtc = dateTimeProvider.UtcNow;
        var expiresAtUtc = nowUtc.AddMinutes(options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("auth_provider", user.AuthProvider.ToString())
        };

        if (user.PersonId.HasValue)
            claims.Add(new Claim("person_id", user.PersonId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AccessTokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(jwt),
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
