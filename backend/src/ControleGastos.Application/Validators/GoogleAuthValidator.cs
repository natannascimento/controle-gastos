using ControleGastos.Application.DTOs;
using FluentValidation;

namespace ControleGastos.Application.Validators;

public class GoogleAuthValidator : AbstractValidator<GoogleAuthDto>
{
    public GoogleAuthValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("IdToken é obrigatório.");
    }
}
