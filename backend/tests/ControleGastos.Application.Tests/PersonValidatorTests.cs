using ControleGastos.Application.DTOs;
using ControleGastos.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace ControleGastos.Application.Tests;

public class PersonValidatorTests
{
    private readonly PersonValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new PersonDto { Name = string.Empty, BirthDate = DateTime.Today.AddYears(-20) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_200_Chars()
    {
        var dto = new PersonDto { Name = new string('a', 201), BirthDate = DateTime.Today.AddYears(-20) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_BirthDate_Is_Today_Or_Future()
    {
        var dto = new PersonDto { Name = "Maria", BirthDate = DateTime.Today };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Dto_Is_Valid()
    {
        var dto = new PersonDto { Name = "Maria", BirthDate = DateTime.Today.AddYears(-30) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
