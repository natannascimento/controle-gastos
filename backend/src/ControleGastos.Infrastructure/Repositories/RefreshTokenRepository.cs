using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Interfaces;
using ControleGastos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        return await context.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x.Person)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public async Task RevokeAllByUserAsync(Guid userId, DateTime nowUtc)
    {
        var tokens = await context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revoke(nowUtc);
        }

        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        context.Entry(refreshToken).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }
}
