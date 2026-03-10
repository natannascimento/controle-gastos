using System.Text.Json;
using ControleGastos.Application.Auth;
using ControleGastos.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ControleGastos.API.Auth;

public class GoogleTokenInfoValidator(HttpClient httpClient, IOptions<GoogleAuthOptions> options) : IGoogleTokenValidator
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            return null;

        using var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<TokenInfoResponse>(stream, _jsonOptions, cancellationToken);
        if (payload is null)
            return null;

        if (!string.Equals(payload.Aud, options.Value.ClientId, StringComparison.Ordinal))
            return null;

        return new GoogleTokenPayload
        {
            Sub = payload.Sub ?? string.Empty,
            Email = payload.Email ?? string.Empty,
            Name = payload.Name ?? string.Empty,
            EmailVerified = string.Equals(payload.EmailVerified, "true", StringComparison.OrdinalIgnoreCase)
        };
    }

    private sealed class TokenInfoResponse
    {
        public string? Aud { get; set; }
        public string? Sub { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? EmailVerified { get; set; }
    }
}
