namespace ControleGastos.API.Auth;

public sealed class RefreshCookieOptions
{
    public string Name { get; set; } = "cg_refresh";
    public string SameSite { get; set; } = "Lax";
    public bool Secure { get; set; }
    public string Path { get; set; } = "/api/auth";
}
