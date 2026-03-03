using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Enums;
using Xunit;

namespace ControleGastos.Domain.Tests;

public class CategoryTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenDescriptionIsEmpty()
    {
        var action = () => new Category("", CategoryPurpose.Expense);

        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Descrição não pode ser vazia.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDescriptionExceedsLimit()
    {
        var description = new string('a', 401);

        var action = () => new Category(description, CategoryPurpose.Expense);

        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Descrição não pode ter mais de 400 caracteres.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldTrimDescription()
    {
        var category = new Category("  Alimentação  ", CategoryPurpose.Expense);

        Assert.Equal("Alimentação", category.Description);
    }
}
