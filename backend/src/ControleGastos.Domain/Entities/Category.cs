using ControleGastos.Domain.Enums;

namespace ControleGastos.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public CategoryPurpose Purpose { get; set; }
}