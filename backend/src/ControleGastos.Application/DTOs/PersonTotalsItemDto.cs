namespace ControleGastos.Application.DTOs;

public sealed class PersonTotalsItemDto
{
    public Guid PersonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
}
