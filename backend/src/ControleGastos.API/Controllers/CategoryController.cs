using ControleGastos.Application.DTOs;
using ControleGastos.Application.Services;
using Microsoft.AspNetCore.Mvc;
using ControleGastos.Domain.Entities;

namespace ControleGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(CategoryService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CategoryDto categoryDto)
    {
        var category = await service.CreateAsync(categoryDto);
        return CreatedAtAction(nameof(GetAll), null, Map(category));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(Map(await service.GetByIdAsync(id)));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok((await service.GetAllAsync()).Select(Map));

    private static CategoryResponseDto Map(Category category)
        => new()
        {
            Id = category.Id,
            Description = category.Description,
            Purpose = category.Purpose
        };
}
