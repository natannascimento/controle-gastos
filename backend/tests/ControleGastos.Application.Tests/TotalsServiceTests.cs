using ControleGastos.Application.Services;
using ControleGastos.Domain.Enums;
using ControleGastos.Domain.Interfaces;
using ControleGastos.Domain.Reports;
using Moq;
using Xunit;

namespace ControleGastos.Application.Tests;

public class TotalsServiceTests
{
    [Fact]
    public async Task GetTotalsByPersonAsync_ShouldReturnItemsAndSummary()
    {
        var reportRepository = new Mock<IReportRepository>();
        reportRepository.Setup(x => x.GetPersonTotalsAsync()).ReturnsAsync(
        [
            new PersonTotals(Guid.NewGuid(), "Ana", 500, 200, 300),
            new PersonTotals(Guid.NewGuid(), "Bruno", 1000, 600, 400)
        ]);

        var service = new TotalsService(reportRepository.Object);

        var result = await service.GetTotalsByPersonAsync();

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1500, result.Summary.TotalIncome);
        Assert.Equal(800, result.Summary.TotalExpense);
        Assert.Equal(700, result.Summary.Balance);
    }

    [Fact]
    public async Task GetTotalsByCategoryAsync_ShouldReturnItemsAndSummary()
    {
        var reportRepository = new Mock<IReportRepository>();
        reportRepository.Setup(x => x.GetCategoryTotalsAsync()).ReturnsAsync(
        [
            new CategoryTotals(Guid.NewGuid(), "Mercado", CategoryPurpose.Expense, 0, 300, -300),
            new CategoryTotals(Guid.NewGuid(), "Salario", CategoryPurpose.Income, 2000, 0, 2000)
        ]);

        var service = new TotalsService(reportRepository.Object);

        var result = await service.GetTotalsByCategoryAsync();

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2000, result.Summary.TotalIncome);
        Assert.Equal(300, result.Summary.TotalExpense);
        Assert.Equal(1700, result.Summary.Balance);
    }
}
