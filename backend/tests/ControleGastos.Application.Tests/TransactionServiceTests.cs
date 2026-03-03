using ControleGastos.Application.DTOs;
using ControleGastos.Application.Errors;
using ControleGastos.Application.Exceptions;
using ControleGastos.Application.Services;
using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Enums;
using ControleGastos.Domain.Interfaces;
using Moq;
using Xunit;

namespace ControleGastos.Application.Tests;

public class TransactionServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldThrowBusinessRule_WhenValueIsNotPositive()
    {
        var personRepository = new Mock<IPersonRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var transactionRepository = new Mock<ITransactionRepository>();
        var service = new TransactionService(personRepository.Object, categoryRepository.Object, transactionRepository.Object);

        var dto = CreateValidDto();
        dto.Value = 0;

        var action = () => service.CreateAsync(dto);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(action);
        Assert.Equal(BusinessErrorMessages.TransactionValueMustBePositive, ex.Message);
        transactionRepository.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenPersonDoesNotExist()
    {
        var personRepository = new Mock<IPersonRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var transactionRepository = new Mock<ITransactionRepository>();

        personRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Person?)null);
        var service = new TransactionService(personRepository.Object, categoryRepository.Object, transactionRepository.Object);

        var action = () => service.CreateAsync(CreateValidDto());

        var ex = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal(BusinessErrorMessages.PersonNotFound, ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFound_WhenCategoryDoesNotExist()
    {
        var person = new Person("Maria", DateTime.Today.AddYears(-30));
        var personRepository = new Mock<IPersonRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var transactionRepository = new Mock<ITransactionRepository>();

        personRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(person);
        categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category)null!);

        var service = new TransactionService(personRepository.Object, categoryRepository.Object, transactionRepository.Object);

        var action = () => service.CreateAsync(CreateValidDto());

        var ex = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal(BusinessErrorMessages.CategoryNotFound, ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowBusinessRule_WhenMinorRegistersIncome()
    {
        var minor = new Person("Pedro", DateTime.Today.AddYears(-15));
        var category = new Category("Receita", CategoryPurpose.Income);

        var personRepository = new Mock<IPersonRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var transactionRepository = new Mock<ITransactionRepository>();

        personRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(minor);
        categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(category);

        var service = new TransactionService(personRepository.Object, categoryRepository.Object, transactionRepository.Object);
        var dto = CreateValidDto();
        dto.TransactionType = TransactionType.Income;

        var action = () => service.CreateAsync(dto);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(action);
        Assert.Equal(BusinessErrorMessages.MinorCannotRegisterIncome, ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowBusinessRule_WhenCategoryIsIncompatibleWithTransactionType()
    {
        var adult = new Person("Ana", DateTime.Today.AddYears(-25));
        var category = new Category("Salario", CategoryPurpose.Income);

        var personRepository = new Mock<IPersonRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var transactionRepository = new Mock<ITransactionRepository>();

        personRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(adult);
        categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(category);

        var service = new TransactionService(personRepository.Object, categoryRepository.Object, transactionRepository.Object);
        var dto = CreateValidDto();
        dto.TransactionType = TransactionType.Expense;

        var action = () => service.CreateAsync(dto);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(action);
        Assert.Equal(BusinessErrorMessages.CategoryIncompatibleWithTransactionType, ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistTransaction_WhenRulesAreSatisfied()
    {
        var adult = new Person("Joao", DateTime.Today.AddYears(-28));
        var category = new Category("Mercado", CategoryPurpose.Expense);

        var personRepository = new Mock<IPersonRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var transactionRepository = new Mock<ITransactionRepository>();

        personRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(adult);
        categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(category);
        transactionRepository.Setup(x => x.CreateAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

        var service = new TransactionService(personRepository.Object, categoryRepository.Object, transactionRepository.Object);

        var id = await service.CreateAsync(CreateValidDto());

        Assert.NotEqual(Guid.Empty, id);
        transactionRepository.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFound_WhenTransactionDoesNotExist()
    {
        var personRepository = new Mock<IPersonRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var transactionRepository = new Mock<ITransactionRepository>();

        transactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Transaction?)null);

        var service = new TransactionService(personRepository.Object, categoryRepository.Object, transactionRepository.Object);

        var action = () => service.GetByIdAsync(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal(BusinessErrorMessages.TransactionNotFound, ex.Message);
    }

    private static CreateTransactionDto CreateValidDto()
    {
        return new CreateTransactionDto
        {
            PersonId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Description = "Compra no mercado",
            Value = 50,
            TransactionType = TransactionType.Expense,
            Date = DateTime.Today
        };
    }
}
