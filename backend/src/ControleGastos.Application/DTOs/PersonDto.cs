namespace ControleGastos.Application.DTOs;

public class PersonDto
{
    // Usado data de nascimento para evitar desatualizacao da idade ao longo do tempo.
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
}
