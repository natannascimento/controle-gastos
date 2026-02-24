using ControleGastos.Application.DTOs;
using ControleGastos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonController(PersonService personService) : ControllerBase
{
    private readonly PersonService _personService = personService;

    [HttpPost]
    public async Task<IActionResult> CreateAsync(PersonDto personDto)
    {
        var personCreated = await _personService.CreateAsync(personDto);
        return CreatedAtAction(nameof(GetById), new { id = personCreated.Id }, personCreated);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _personService.GetByIdAsync(id));
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _personService.GetAllAsync());
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PersonDto personDto)
    {
        await _personService.UpdateAsync(id, personDto);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _personService.DeleteAsync(id);
        return NoContent();
    }
}