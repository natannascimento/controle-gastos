namespace ControleGastos.Application.DTOs;

public sealed class PersonResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
}
