namespace ControleGastos.Application.Errors;

public static class BusinessErrorMessages
{
    public const string PersonNotFound = "Pessoa não encontrada.";
    public const string CategoryNotFound = "Categoria não encontrada.";
    public const string TransactionNotFound = "Transação não encontrada.";
    public const string TransactionValueMustBePositive = "O valor da transação deve ser maior que zero.";
    public const string MinorCannotRegisterIncome = "Menores de idade só podem registrar despesas.";
    public const string CategoryIncompatibleWithTransactionType = "Categoria incompatível com o tipo da transação.";
    public const string EmailAlreadyInUse = "Este email já está em uso.";
    public const string InvalidCredentials = "Credenciais inválidas.";
    public const string InvalidGoogleToken = "Token do Google inválido.";
    public const string GoogleEmailNotVerified = "Conta Google com email não verificado.";
    public const string InvalidRefreshToken = "Refresh token inválido.";
    public const string UserNotFound = "Usuário não encontrado.";
}
