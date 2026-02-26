using ControleGastos.Domain.Enums;

namespace ControleGastos.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Value { get; private set; }
    public DateTime Date { get; private set; }
    public Guid PersonId { get; private set; }
    public Guid CategoryId { get; private set; }
    public TransactionType Type { get; private set; }
    public Person Person { get; private set; } = null!;
    public Category Category { get; private set; } = null!;
    
    private Transaction() { }

    public Transaction(Guid personId, Guid categoryId, string description, decimal value, TransactionType type, DateTime date)
    {
        Id = Guid.NewGuid();
        PersonId = personId;
        CategoryId = categoryId;
        Description = description;
        Value = value;
        Type = type;
        Date = date;
    }
}
