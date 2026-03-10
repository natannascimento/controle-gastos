using System.Security.Cryptography;
using System.Text;
using ControleGastos.Application.Auth;
using ControleGastos.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ControleGastos.API.Auth;

public class Sha256RefreshTokenFactory(IOptions<JwtOptions> jwtOptions, IDateTimeProvider dateTimeProvider) : IRefreshTokenFactory
{
    public GeneratedRefreshToken Generate()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new GeneratedRefreshToken
        {
            RawToken = rawToken,
            Hash = Hash(rawToken),
            ExpiresAtUtc = dateTimeProvider.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays)
        };
    }

    public string Hash(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
