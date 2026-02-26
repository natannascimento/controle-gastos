using ControleGastos.Domain.Enums;

namespace ControleGastos.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public CategoryPurpose Purpose { get; private set; }
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    
    private Category() { }
    
    public Category(string description, CategoryPurpose purpose)
    {
        Id = Guid.NewGuid();
        SetDescription(description);
        SetPurpose(purpose);
    }

    private void SetDescription(string description)
    {
        // Regra: descrição obrigatória e limitada a 400 caracteres.
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Descrição não pode ser vazia.");

        if (description.Length > 400)
            throw new ArgumentException("Descrição não pode ter mais de 400 caracteres.");

        Description = description.Trim();
    }

    private void SetPurpose(CategoryPurpose purpose)
    {
        // Regra: finalidade deve ser válida (Despesa/Receita/Ambas).
        if (!Enum.IsDefined(typeof(CategoryPurpose), purpose))
            throw new ArgumentException("Finalidade categoraia invalida.");

        Purpose = purpose;
    }
    
}
