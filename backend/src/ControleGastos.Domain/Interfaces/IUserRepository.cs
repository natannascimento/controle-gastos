using ControleGastos.Domain.Entities;

namespace ControleGastos.Domain.Interfaces;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByGoogleSubAsync(string googleSub);
    Task<User> UpdateAsync(User user);
}
