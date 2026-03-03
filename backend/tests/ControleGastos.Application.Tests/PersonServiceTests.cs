using ControleGastos.Application.DTOs;
using ControleGastos.Application.Errors;
using ControleGastos.Application.Exceptions;
using ControleGastos.Application.Services;
using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Interfaces;
using Moq;
using Xunit;

namespace ControleGastos.Application.Tests;

public class PersonServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFound_WhenPersonDoesNotExist()
    {
        var repository = new Mock<IPersonRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Person?)null);
        var service = new PersonService(repository.Object);

        var action = () => service.GetByIdAsync(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal(BusinessErrorMessages.PersonNotFound, ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePerson_WhenPersonExists()
    {
        var existingPerson = new Person("Nome Antigo", DateTime.Today.AddYears(-30));
        var repository = new Mock<IPersonRepository>();
        repository.Setup(x => x.GetByIdAsync(existingPerson.Id)).ReturnsAsync(existingPerson);
        repository.Setup(x => x.UpdateAsync(existingPerson)).ReturnsAsync(existingPerson);

        var service = new PersonService(repository.Object);
        var dto = new PersonDto { Name = "Novo Nome", BirthDate = DateTime.Today.AddYears(-25) };

        await service.UpdateAsync(existingPerson.Id, dto);

        repository.Verify(x => x.UpdateAsync(It.Is<Person>(p =>
            p.Id == existingPerson.Id &&
            p.Name == dto.Name &&
            p.BirthDate == dto.BirthDate)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete_WhenPersonExists()
    {
        var person = new Person("Maria", DateTime.Today.AddYears(-20));
        var repository = new Mock<IPersonRepository>();
        repository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        repository.Setup(x => x.DeleteAsync(person)).Returns(Task.CompletedTask);

        var service = new PersonService(repository.Object);

        await service.DeleteAsync(person.Id);

        repository.Verify(x => x.DeleteAsync(person), Times.Once);
    }
}
