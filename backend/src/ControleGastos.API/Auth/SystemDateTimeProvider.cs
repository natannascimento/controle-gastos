using ControleGastos.Application.Interfaces;

namespace ControleGastos.API.Auth;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
