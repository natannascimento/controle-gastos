using FluentValidation;
using ControleGastos.Application.DTOs;

namespace ControleGastos.Application.Validators;

public class PersonValidator : AbstractValidator<PersonDto>
{
    public PersonValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O Nome é Obrigatório")
            .MaximumLength(200).WithMessage("O Nome deve ter no máximo 200 caracteres.");
    }
    
}