using ControleGastos.API.Auth;
using ControleGastos.Application.DTOs;
using ControleGastos.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ControleGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService, IOptions<RefreshCookieOptions> cookieOptions) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRegisterDto dto)
    {
        var session = await authService.RegisterAsync(dto);
        WriteRefreshCookie(session.RefreshToken, session.RefreshTokenExpiresAtUtc);
        return Ok(session.Response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginDto dto)
    {
        var session = await authService.LoginAsync(dto);
        WriteRefreshCookie(session.RefreshToken, session.RefreshTokenExpiresAtUtc);
        return Ok(session.Response);
    }

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<IActionResult> Google([FromBody] GoogleAuthDto dto, CancellationToken cancellationToken)
    {
        var session = await authService.LoginWithGoogleAsync(dto, cancellationToken);
        WriteRefreshCookie(session.RefreshToken, session.RefreshTokenExpiresAtUtc);
        return Ok(session.Response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[cookieOptions.Value.Name] ?? string.Empty;
        var session = await authService.RefreshAsync(refreshToken);
        WriteRefreshCookie(session.RefreshToken, session.RefreshTokenExpiresAtUtc);
        return Ok(session.Response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[cookieOptions.Value.Name] ?? string.Empty;
        await authService.LogoutAsync(refreshToken);

        Response.Cookies.Delete(cookieOptions.Value.Name, new CookieOptions
        {
            HttpOnly = true,
            Secure = cookieOptions.Value.Secure,
            SameSite = ParseSameSite(cookieOptions.Value.SameSite),
            Path = cookieOptions.Value.Path
        });

        return NoContent();
    }

    private void WriteRefreshCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(cookieOptions.Value.Name, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = cookieOptions.Value.Secure,
            SameSite = ParseSameSite(cookieOptions.Value.SameSite),
            Path = cookieOptions.Value.Path,
            Expires = new DateTimeOffset(expiresAtUtc)
        });
    }

    private static SameSiteMode ParseSameSite(string sameSite) => sameSite.ToLowerInvariant() switch
    {
        "strict" => SameSiteMode.Strict,
        "none" => SameSiteMode.None,
        _ => SameSiteMode.Lax
    };
}
