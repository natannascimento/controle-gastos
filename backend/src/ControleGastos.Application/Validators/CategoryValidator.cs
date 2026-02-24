using ControleGastos.Application.DTOs;
using FluentValidation;

namespace ControleGastos.Application.Validators;

public class CategoryValidator : AbstractValidator<CategoryDto>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(400).WithMessage("A descrição deve ter no máximo 400 caracteres.");
        RuleFor(x => x.Purpose)
            .IsInEnum().WithMessage("Finalidade Invalida");       
    }
    
}