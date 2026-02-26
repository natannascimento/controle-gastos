using ControleGastos.Domain.Enums;
namespace ControleGastos.Application.DTOs;

public class CreateTransactionDto
{
    public Guid PersonId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public Guid CategoryId { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime Date { get; set; }
}