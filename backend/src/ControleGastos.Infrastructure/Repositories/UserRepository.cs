using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Interfaces;
using ControleGastos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User> CreateAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .Include(x => x.Person)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await context.Users
            .Include(x => x.Person)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail);
    }

    public async Task<User?> GetByGoogleSubAsync(string googleSub)
    {
        return await context.Users
            .Include(x => x.Person)
            .FirstOrDefaultAsync(x => x.GoogleSub == googleSub);
    }

    public async Task<User> UpdateAsync(User user)
    {
        context.Entry(user).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return user;
    }
}
