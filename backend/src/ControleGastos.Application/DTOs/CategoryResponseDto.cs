using ControleGastos.Domain.Enums;

namespace ControleGastos.Application.DTOs;

public sealed class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public CategoryPurpose Purpose { get; set; }
}
