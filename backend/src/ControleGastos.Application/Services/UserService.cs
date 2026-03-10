using ControleGastos.Application.DTOs;
using ControleGastos.Application.Errors;
using ControleGastos.Application.Exceptions;
using ControleGastos.Application.Interfaces;
using ControleGastos.Domain.Interfaces;

namespace ControleGastos.Application.Services;

public class UserService(IUserRepository userRepository, IUserContext userContext)
{
    public async Task<AuthUserDto> GetMeAsync()
    {
        if (!userContext.UserId.HasValue)
            throw new NotFoundException(BusinessErrorMessages.UserNotFound);

        var user = await userRepository.GetByIdAsync(userContext.UserId.Value)
            ?? throw new NotFoundException(BusinessErrorMessages.UserNotFound);

        return new AuthUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            AuthProvider = user.AuthProvider,
            PersonId = user.PersonId
        };
    }
}
