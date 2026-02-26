namespace ControleGastos.Domain.Reports;

public sealed record PersonTotals(
    Guid PersonId,
    string Name,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance
);
