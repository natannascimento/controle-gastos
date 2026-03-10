using ControleGastos.Application.DTOs;
using FluentValidation;

namespace ControleGastos.Application.Validators;

public class AuthLoginValidator : AbstractValidator<AuthLoginDto>
{
    public AuthLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
