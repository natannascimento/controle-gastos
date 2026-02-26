using ControleGastos.Application.DTOs;
using ControleGastos.Application.Errors;
using ControleGastos.Application.Exceptions;
using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Interfaces;

namespace ControleGastos.Application.Services;

public class CategoryService(ICategoryRepository repository)
{
    public async Task<Category> CreateAsync(CategoryDto dto)
    {
        var category = new Category(dto.Description, dto.Purpose);
        return await repository.CreateAsync(category);
    }

    public async Task<Category> GetByIdAsync(Guid id)
        => await repository.GetByIdAsync(id)
           ?? throw new NotFoundException(BusinessErrorMessages.CategoryNotFound);
    
    public async Task<IEnumerable<Category>> GetAllAsync()
        => await repository.GetAllAsync();
}
