using ControleGastos.Application.DTOs;
using FluentValidation;

namespace ControleGastos.Application.Validators;

public class CategoryValidator : AbstractValidator<CategoryDto>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(400);
        RuleFor(x => x.Purpose)
            .IsInEnum();       
    }
    
}