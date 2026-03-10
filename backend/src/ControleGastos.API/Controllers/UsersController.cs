using ControleGastos.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
        => Ok(await userService.GetMeAsync());
}
