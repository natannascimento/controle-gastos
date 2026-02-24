using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Interfaces;
using ControleGastos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Infrastructure.Repositories;

public class PersonRepository(AppDbContext context) : IPersonRepository
{
    public async Task<Person> CreateAsync(Person person)
    {
        await context.People.AddAsync(person);
        await context.SaveChangesAsync();
        return person;
    }
    
    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        return await context.People
            .Include(p => p.Transactions)
            .ToListAsync();
    }
    
    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await context.People
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<Person> UpdateAsync(Person person)
    {
        context.Entry(person).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return person;
    }
    
    public async Task DeleteAsync(Person person)
    {
        context.People.Remove(person);
        await context.SaveChangesAsync();
    }
}