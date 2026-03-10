namespace ControleGastos.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
