using ControleGastos.Domain.Entities;

namespace ControleGastos.Domain.Interfaces;

public interface ITransactionRepository
{
    Task CreateAsync(Transaction transaction);
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetAllAsync();
    
}