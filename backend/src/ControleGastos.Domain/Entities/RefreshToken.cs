namespace ControleGastos.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsActive(DateTime nowUtc) => RevokedAt is null && ExpiresAt > nowUtc;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAt, DateTime createdAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId obrigatorio.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("TokenHash obrigatorio.", nameof(tokenHash));
        if (expiresAt <= createdAt)
            throw new ArgumentException("ExpiresAt deve ser maior que CreatedAt.", nameof(expiresAt));

        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public void Revoke(DateTime nowUtc)
    {
        RevokedAt ??= nowUtc;
    }
}
