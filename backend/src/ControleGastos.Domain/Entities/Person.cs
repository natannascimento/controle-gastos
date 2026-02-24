namespace ControleGastos.Domain.Entities;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}