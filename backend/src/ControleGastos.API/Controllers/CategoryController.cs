using ControleGastos.Application.DTOs;
using ControleGastos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(CategoryService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CategoryDto categoryDto)
    {
        var category = await service.CreateAsync(categoryDto);
        return Created("", category);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await service.GetAllAsync());
}