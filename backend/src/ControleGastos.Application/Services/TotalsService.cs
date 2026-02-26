using ControleGastos.Application.DTOs;
using ControleGastos.Domain.Interfaces;

namespace ControleGastos.Application.Services;

public class TotalsService(IReportRepository repository)
{
    public async Task<PersonTotalsResponseDto> GetTotalsByPersonAsync()
    {
        var items = await repository.GetPersonTotalsAsync();
        // Resumo geral deve ser exibido ao final da listagem.
        var mapped = items
            .Select(x => new PersonTotalsItemDto
            {
                PersonId = x.PersonId,
                Name = x.Name,
                TotalIncome = x.TotalIncome,
                TotalExpense = x.TotalExpense,
                Balance = x.Balance
            })
            .ToList();

        return new PersonTotalsResponseDto
        {
            Items = mapped,
            Summary = new TotalsSummaryDto
            {
                TotalIncome = mapped.Sum(x => x.TotalIncome),
                TotalExpense = mapped.Sum(x => x.TotalExpense),
                Balance = mapped.Sum(x => x.Balance)
            }
        };
    }

    public async Task<CategoryTotalsResponseDto> GetTotalsByCategoryAsync()
    {
        var items = await repository.GetCategoryTotalsAsync();
        // Resumo geral deve ser exibido ao final da listagem.
        var mapped = items
            .Select(x => new CategoryTotalsItemDto
            {
                CategoryId = x.CategoryId,
                Description = x.Description,
                Purpose = x.Purpose,
                TotalIncome = x.TotalIncome,
                TotalExpense = x.TotalExpense,
                Balance = x.Balance
            })
            .ToList();

        return new CategoryTotalsResponseDto
        {
            Items = mapped,
            Summary = new TotalsSummaryDto
            {
                TotalIncome = mapped.Sum(x => x.TotalIncome),
                TotalExpense = mapped.Sum(x => x.TotalExpense),
                Balance = mapped.Sum(x => x.Balance)
            }
        };
    }
}
