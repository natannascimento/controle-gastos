using ControleGastos.Application.DTOs;
using ControleGastos.Domain.Entities;
using ControleGastos.Domain.Enums;
using ControleGastos.Domain.Interfaces;

namespace ControleGastos.Application.Services;

public class TransactionService(IPersonRepository personRepository,
                                ICategoryRepository categoryRepository,
                                ITransactionRepository transactionRepository)
{
    public async Task<Guid> CreateAsync(CreateTransactionDto createTransactionDto)
    {
        // Regras de negócio centrais da transação:
        // 1) Valor deve ser positivo.
        // 2) Pessoa e categoria devem existir.
        // 3) Menor de idade só pode registrar despesa.
        // 4) Categoria deve ser compatível com o tipo (despesa/receita/ambas).
        if (createTransactionDto.Value <= 0)
            throw new Exception("O valor da transação deve ser maior que zero.");

        var person = await personRepository.GetByIdAsync(createTransactionDto.PersonId);
        if (person == null)
            throw new Exception("Pessoa não encontrada.");

        var category = await categoryRepository.GetByIdAsync(createTransactionDto.CategoryId);
        if (category == null)
            throw new Exception("Categoria não encontrada.");
        
        if (person.Age < 18 && createTransactionDto.TransactionType == TransactionType.Income)
            throw new Exception("Menores de idade só podem registrar despesas.");
        
        if (!ValidCategoryToTransactionType(category, createTransactionDto.TransactionType))
            throw new Exception("Categoria incompatível com o tipo da transação.");

        var transaction = new Transaction(
            createTransactionDto.PersonId,
            createTransactionDto.CategoryId,
            createTransactionDto.Description,
            createTransactionDto.Value,
            createTransactionDto.TransactionType,
            createTransactionDto.Date
        );

        await transactionRepository.CreateAsync(transaction);

        return transaction.Id;
    }

    public async Task<Transaction> GetByIdAsync(Guid id)
        => await transactionRepository.GetByIdAsync(id)
           ?? throw new Exception("Transação não encontrada.");
   
    
    public async Task<IEnumerable<Transaction>> GetAllAsync()
        => await transactionRepository.GetAllAsync();
    
    private static bool ValidCategoryToTransactionType(Category category, TransactionType transactionType)
    {
        switch (category.Purpose)
        {
            case CategoryPurpose.Both:
            case CategoryPurpose.Expense when transactionType == TransactionType.Expense:
            case CategoryPurpose.Income when transactionType == TransactionType.Income:
                return true;
            default:
                return false;
        }
    }
}
