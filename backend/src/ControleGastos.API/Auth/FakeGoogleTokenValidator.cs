using ControleGastos.Application.Auth;
using ControleGastos.Application.Interfaces;

namespace ControleGastos.API.Auth;

public class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    public Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken) || !idToken.StartsWith("test:", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<GoogleTokenPayload?>(null);

        var parts = idToken.Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length < 4)
            return Task.FromResult<GoogleTokenPayload?>(null);

        return Task.FromResult<GoogleTokenPayload?>(new GoogleTokenPayload
        {
            Sub = parts[1],
            Email = parts[2],
            Name = parts[3],
            EmailVerified = true
        });
    }
}
