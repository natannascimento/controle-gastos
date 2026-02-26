using ControleGastos.Domain.Enums;
using ControleGastos.Domain.Interfaces;
using ControleGastos.Domain.Reports;
using ControleGastos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Infrastructure.Repositories;

public class ReportRepository(AppDbContext context) : IReportRepository
{
    public async Task<IReadOnlyList<PersonTotals>> GetPersonTotalsAsync()
    {
        return await context.People
            .Select(p => new PersonTotals(
                p.Id,
                p.Name,
                p.Transactions.Where(t => t.Type == TransactionType.Income)
                    .Sum(t => (decimal?)t.Value) ?? 0m,
                p.Transactions.Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => (decimal?)t.Value) ?? 0m,
                (p.Transactions.Where(t => t.Type == TransactionType.Income)
                     .Sum(t => (decimal?)t.Value) ?? 0m)
                - (p.Transactions.Where(t => t.Type == TransactionType.Expense)
                     .Sum(t => (decimal?)t.Value) ?? 0m)
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CategoryTotals>> GetCategoryTotalsAsync()
    {
        return await context.Categories
            .Select(c => new CategoryTotals(
                c.Id,
                c.Description,
                c.Purpose,
                c.Transactions.Where(t => t.Type == TransactionType.Income)
                    .Sum(t => (decimal?)t.Value) ?? 0m,
                c.Transactions.Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => (decimal?)t.Value) ?? 0m,
                (c.Transactions.Where(t => t.Type == TransactionType.Income)
                     .Sum(t => (decimal?)t.Value) ?? 0m)
                - (c.Transactions.Where(t => t.Type == TransactionType.Expense)
                     .Sum(t => (decimal?)t.Value) ?? 0m)
            ))
            .ToListAsync();
    }
}
