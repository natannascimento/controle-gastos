using ControleGastos.Application.DTOs;
using ControleGastos.Application.Errors;
using ControleGastos.Application.Exceptions;
using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Interfaces;

namespace ControleGastos.Application.Services;

public class PersonService(IPersonRepository personRepository)
{
    public async Task<Person> CreateAsync(PersonDto personDto )
    {
        var personCreated = new Person(personDto.Name, personDto.BirthDate);
        return await personRepository.CreateAsync(personCreated);
    }
    
    public async Task<IEnumerable<Person>> GetAllAsync() 
        => await personRepository.GetAllAsync();

    public async Task<Person> GetByIdAsync(Guid id)
        => await personRepository.GetByIdAsync(id) 
           ?? throw new NotFoundException(BusinessErrorMessages.PersonNotFound);
    
    public async Task UpdateAsync(Guid id, PersonDto personDto)
    {
        var person = await GetByIdAsync(id);
        person.SetName(personDto.Name);
        person.SetBirthDate(personDto.BirthDate);

        await personRepository.UpdateAsync(person);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var person = await GetByIdAsync(id);
        await personRepository.DeleteAsync(person);
    }
}
