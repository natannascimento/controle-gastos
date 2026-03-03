using ControleGastos.Application.DTOs;
using ControleGastos.Application.Validators;
using ControleGastos.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace ControleGastos.Application.Tests;

public class CategoryValidatorTests
{
    private readonly CategoryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        var dto = new CategoryDto { Description = string.Empty, Purpose = CategoryPurpose.Expense };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_400_Chars()
    {
        var dto = new CategoryDto { Description = new string('a', 401), Purpose = CategoryPurpose.Expense };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Purpose_Is_Invalid()
    {
        var dto = new CategoryDto { Description = "Mercado", Purpose = (CategoryPurpose)999 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Purpose);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Dto_Is_Valid()
    {
        var dto = new CategoryDto { Description = "Mercado", Purpose = CategoryPurpose.Expense };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
