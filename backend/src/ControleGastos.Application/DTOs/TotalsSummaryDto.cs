namespace ControleGastos.Application.DTOs;

public sealed class TotalsSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
}
