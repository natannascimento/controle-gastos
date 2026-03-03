using ControleGastos.Application.DTOs;
using ControleGastos.Application.Validators;
using ControleGastos.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace ControleGastos.Application.Tests;

public class TransactionValidatorTests
{
    private readonly TransactionValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_PersonId_Is_Empty()
    {
        var dto = CreateValidDto();
        dto.PersonId = Guid.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PersonId);
    }

    [Fact]
    public void Should_Have_Error_When_CategoryId_Is_Empty()
    {
        var dto = CreateValidDto();
        dto.CategoryId = Guid.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        var dto = CreateValidDto();
        dto.Description = string.Empty;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Value_Is_Not_Positive()
    {
        var dto = CreateValidDto();
        dto.Value = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Should_Have_Error_When_TransactionType_Is_Invalid()
    {
        var dto = CreateValidDto();
        dto.TransactionType = (TransactionType)999;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.TransactionType);
    }

    [Fact]
    public void Should_Have_Error_When_Date_Is_Default()
    {
        var dto = CreateValidDto();
        dto.Date = default;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Dto_Is_Valid()
    {
        var dto = CreateValidDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreateTransactionDto CreateValidDto()
    {
        return new CreateTransactionDto
        {
            PersonId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = "Compra",
            Value = 10,
            TransactionType = TransactionType.Expense,
            Date = DateTime.Today
        };
    }
}
