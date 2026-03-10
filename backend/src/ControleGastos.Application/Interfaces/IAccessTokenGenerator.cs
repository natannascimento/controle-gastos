using ControleGastos.Application.Auth;
using ControleGastos.Domain.Entities;

namespace ControleGastos.Application.Interfaces;

public interface IAccessTokenGenerator
{
    AccessTokenResult Generate(User user);
}
