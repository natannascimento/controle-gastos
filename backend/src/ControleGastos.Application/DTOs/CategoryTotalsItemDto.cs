using ControleGastos.Domain.Enums;

namespace ControleGastos.Application.DTOs;

public sealed class CategoryTotalsItemDto
{
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public CategoryPurpose Purpose { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
}
