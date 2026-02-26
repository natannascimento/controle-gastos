using ControleGastos.Domain.Enums;

namespace ControleGastos.Application.DTOs;

public sealed class TransactionResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
    public Guid PersonId { get; set; }
    public Guid CategoryId { get; set; }
    public TransactionType Type { get; set; }
}
