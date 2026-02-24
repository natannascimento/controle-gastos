using ControleGastos.Domain.Enums;

namespace ControleGastos.Application.DTOs;

public class CategoryDto
{
    public string Description { get; set; } = string.Empty;
    public CategoryPurpose Purpose { get; set; }
}