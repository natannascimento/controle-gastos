namespace ControleGastos.Application.Errors;

public static class BusinessErrorMessages
{
    public const string PersonNotFound = "Pessoa não encontrada.";
    public const string CategoryNotFound = "Categoria não encontrada.";
    public const string TransactionNotFound = "Transação não encontrada.";
    public const string TransactionValueMustBePositive = "O valor da transação deve ser maior que zero.";
    public const string MinorCannotRegisterIncome = "Menores de idade só podem registrar despesas.";
    public const string CategoryIncompatibleWithTransactionType = "Categoria incompatível com o tipo da transação.";
}
