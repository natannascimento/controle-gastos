using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Enums;
using Xunit;

namespace ControleGastos.Domain.Tests;

public class TransactionTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenPersonIdIsEmpty()
    {
        var action = () => new Transaction(
            Guid.Empty,
            Guid.NewGuid(),
            "Compra",
            100,
            TransactionType.Expense,
            DateTime.Today);

        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Pessoa obrigatoria.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenValueIsInvalid()
    {
        var action = () => new Transaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Compra",
            0,
            TransactionType.Expense,
            DateTime.Today);

        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Valor deve ser maior que zero.", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldCreateTransaction_WhenDataIsValid()
    {
        var personId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var date = DateTime.Today;

        var transaction = new Transaction(
            personId,
            categoryId,
            "Supermercado",
            150.75m,
            TransactionType.Expense,
            date);

        Assert.Equal(personId, transaction.PersonId);
        Assert.Equal(categoryId, transaction.CategoryId);
        Assert.Equal("Supermercado", transaction.Description);
        Assert.Equal(150.75m, transaction.Value);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(date, transaction.Date);
    }
}
