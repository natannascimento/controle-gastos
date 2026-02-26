using ControleGastos.Application.DTOs;
using FluentValidation;

namespace ControleGastos.Application.Validators;

public class TransactionValidator : AbstractValidator<CreateTransactionDto>
{
    public TransactionValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Pessoa obrigatoria.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria obrigatoria.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descricao obrigatoria.")
            .MaximumLength(400).WithMessage("Descricao deve ter no maximo 400 caracteres.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Tipo de transacao invalido.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Data obrigatoria.");
    }
}
