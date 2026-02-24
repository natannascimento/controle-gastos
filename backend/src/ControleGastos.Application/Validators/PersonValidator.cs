using FluentValidation;
using ControleGastos.Application.DTOs;

namespace ControleGastos.Application.Validators;

public class PersonValidator : AbstractValidator<PersonDto>
{
    public PersonValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
    
}