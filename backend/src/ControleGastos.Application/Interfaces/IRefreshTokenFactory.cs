using ControleGastos.Application.Auth;

namespace ControleGastos.Application.Interfaces;

public interface IRefreshTokenFactory
{
    GeneratedRefreshToken Generate();
    string Hash(string rawToken);
}
