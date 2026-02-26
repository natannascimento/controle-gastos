using ControleGastos.Domain.Reports;

namespace ControleGastos.Domain.Interfaces;

public interface IReportRepository
{
    Task<IReadOnlyList<PersonTotals>> GetPersonTotalsAsync();
    Task<IReadOnlyList<CategoryTotals>> GetCategoryTotalsAsync();
}
