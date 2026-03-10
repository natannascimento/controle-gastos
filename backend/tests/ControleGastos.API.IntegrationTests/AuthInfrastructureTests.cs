using ControleGastos.API.Auth;
using ControleGastos.Application.Interfaces;
using ControleGastos.Domain.Entities;
using Microsoft.Extensions.Options;
using Xunit;

namespace ControleGastos.API.IntegrationTests;

public class AuthInfrastructureTests
{
    [Fact]
    public void PasswordHasher_ShouldHashAndVerify()
    {
        var hasher = new Pbkdf2PasswordHasher();

        var hash = hasher.Hash("12345678");

        Assert.True(hasher.Verify("12345678", hash));
        Assert.False(hasher.Verify("wrong", hash));
    }

    [Fact]
    public void JwtAccessTokenGenerator_ShouldGenerateTokenWithExpiry()
    {
        var now = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc);
        var options = Options.Create(new JwtOptions
        {
            Issuer = "issuer",
            Audience = "audience",
            Secret = "a-super-secret-key-with-32-char-min!!",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });

        var generator = new JwtAccessTokenGenerator(options, new FixedDateTimeProvider(now));
        var user = User.CreateWithPassword("john@test.com", "John", "hash", null, now);

        var result = generator.Generate(user);

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal(now.AddMinutes(15), result.ExpiresAtUtc);
    }

    [Fact]
    public void RefreshTokenFactory_ShouldGenerateAndHashToken()
    {
        var now = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc);
        var options = Options.Create(new JwtOptions
        {
            Issuer = "issuer",
            Audience = "audience",
            Secret = "a-super-secret-key-with-32-char-min!!",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });

        var factory = new Sha256RefreshTokenFactory(options, new FixedDateTimeProvider(now));

        var generated = factory.Generate();

        Assert.False(string.IsNullOrWhiteSpace(generated.RawToken));
        Assert.Equal(factory.Hash(generated.RawToken), generated.Hash);
        Assert.Equal(now.AddDays(7), generated.ExpiresAtUtc);
    }

    private sealed class FixedDateTimeProvider(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }
}
