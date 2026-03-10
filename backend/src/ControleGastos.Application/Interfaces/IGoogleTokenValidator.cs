using ControleGastos.Application.Auth;

namespace ControleGastos.Application.Interfaces;

public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
