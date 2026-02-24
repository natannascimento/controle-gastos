namespace ControleGastos.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;
}