using ControleGastos.Domain.Enums;

namespace ControleGastos.Application.DTOs;

public sealed class AuthUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AuthProvider AuthProvider { get; set; }
    public Guid? PersonId { get; set; }
}
