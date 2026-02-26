using ControleGastos.Domain.Enums;

namespace ControleGastos.Domain.Reports;

public sealed record CategoryTotals(
    Guid CategoryId,
    string Description,
    CategoryPurpose Purpose,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance
);
