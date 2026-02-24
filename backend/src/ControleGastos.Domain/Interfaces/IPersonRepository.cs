using ControleGastos.Domain.Entities;

namespace ControleGastos.Domain.Interfaces;

public interface IPersonRepository
{
    Task<Person> CreateAsync(Person person);
    Task<IEnumerable<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(Guid id);
    Task<Person> UpdateAsync(Person person);
    Task DeleteAsync(Person person);
}