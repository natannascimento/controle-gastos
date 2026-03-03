using ControleGastos.Domain.Entities;
using Xunit;

namespace ControleGastos.Domain.Tests;

public class PersonTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsEmpty()
    {
        var birthDate = DateTime.Today.AddYears(-30);

        var action = () => new Person("", birthDate);

        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Nome não pode ser vazio", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenBirthDateIsFuture()
    {
        var futureDate = DateTime.Today.AddDays(1);

        var action = () => new Person("Maria", futureDate);

        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal("Data de Nascimento não pode ser futura", ex.Message);
    }

    [Fact]
    public void Age_ShouldBeCalculatedCorrectly()
    {
        var birthDate = DateTime.Today.AddYears(-20);
        var person = new Person("Carlos", birthDate);

        Assert.Equal(20, person.Age);
    }
}
