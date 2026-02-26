using ControleGastos.Application.DTOs;
using ControleGastos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController(PersonService personService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(PersonDto personDto)
    {
        var personCreated = await personService.CreateAsync(personDto);
        return CreatedAtAction(nameof(GetById), new { id = personCreated.Id }, Map(personCreated));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(Map(await personService.GetByIdAsync(id)));
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok((await personService.GetAllAsync()).Select(Map));
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PersonDto personDto)
    {
        await personService.UpdateAsync(id, personDto);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await personService.DeleteAsync(id);
        return NoContent();
    }

    private static PersonResponseDto Map(ControleGastos.Domain.Entities.Person person)
        => new()
        {
            Id = person.Id,
            Name = person.Name,
            BirthDate = person.BirthDate,
            Age = person.Age
        };
}
