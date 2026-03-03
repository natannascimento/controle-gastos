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
        SetPersonId(personId);
        SetCategoryId(categoryId);
        SetDescription(description);
        SetValue(value);
        SetType(type);
        SetDate(date);
    }

    private void SetPersonId(Guid personId)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("Pessoa obrigatoria.");

        PersonId = personId;
    }

    private void SetCategoryId(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Categoria obrigatoria.");

        CategoryId = categoryId;
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Descricao obrigatoria.");

        if (description.Length > 400)
            throw new ArgumentException("Descricao deve ter no maximo 400 caracteres.");

        Description = description.Trim();
    }

    private void SetValue(decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.");

        Value = value;
    }

    private void SetType(TransactionType type)
    {
        if (!Enum.IsDefined(typeof(TransactionType), type))
            throw new ArgumentException("Tipo de transacao invalido.");

        Type = type;
    }

    private void SetDate(DateTime date)
    {
        if (date == default)
            throw new ArgumentException("Data obrigatoria.");

        Date = date;
    }
}
