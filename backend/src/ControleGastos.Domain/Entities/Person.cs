namespace ControleGastos.Domain.Entities;

public class Person
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }

    public int Age => CalculateAge();
    public User? User { get; private set; }

    private readonly List<Transaction> _transactions = [];
    public IReadOnlyCollection<Transaction> Transactions => _transactions;
    
    private Person() { }
    public Person(string name, DateTime birthDate)
    {
        SetName(name);
        SetBirthDate(birthDate);
        Id = Guid.NewGuid();
    }
    
    public void SetName(string name)
    {
        // Regra: nome obrigatório.
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome não pode ser vazio");

        Name = name;
    }

    public void SetBirthDate(DateTime birthDate)
    {
        // Regra: data de nascimento não pode ser futura.
        if (birthDate > DateTime.Today)
            throw new ArgumentException("Data de Nascimento não pode ser futura");

        BirthDate = birthDate;
    }
    
    private int CalculateAge()
    {
        var today = DateTime.Today;
        var age = today.Year - BirthDate.Year;

        if (BirthDate.Date > today.AddYears(-age))
            age--;

        return age;
    }
}
