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

public class CategoryServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFound_WhenCategoryDoesNotExist()
    {
        var repository = new Mock<ICategoryRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category)null!);
        var service = new CategoryService(repository.Object);

        var action = () => service.GetByIdAsync(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal(BusinessErrorMessages.CategoryNotFound, ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCategory_WhenDtoIsValid()
    {
        var repository = new Mock<ICategoryRepository>();
        repository.Setup(x => x.CreateAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => c);

        var service = new CategoryService(repository.Object);
        var dto = new CategoryDto { Description = "  Mercado  ", Purpose = CategoryPurpose.Expense };

        var created = await service.CreateAsync(dto);

        Assert.Equal("Mercado", created.Description);
        Assert.Equal(CategoryPurpose.Expense, created.Purpose);
        repository.Verify(x => x.CreateAsync(It.IsAny<Category>()), Times.Once);
    }
}
