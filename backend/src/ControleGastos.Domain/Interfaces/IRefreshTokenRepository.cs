using ControleGastos.Domain.Entities;

namespace ControleGastos.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task RevokeAllByUserAsync(Guid userId, DateTime nowUtc);
    Task UpdateAsync(RefreshToken refreshToken);
}
