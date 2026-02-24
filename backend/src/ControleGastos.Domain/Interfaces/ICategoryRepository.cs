using ControleGastos.Domain.Entities;

namespace ControleGastos.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category> CreateAsync(Category category);
    Task<IEnumerable<Category>> GetAllAsync();
}