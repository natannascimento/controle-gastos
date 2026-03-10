using ControleGastos.Domain.Enums;

namespace ControleGastos.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AuthProvider AuthProvider { get; private set; }
    public string? GoogleSub { get; private set; }
    public string? PasswordHash { get; private set; }
    public Guid? PersonId { get; private set; }
    public Person? Person { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private User() { }

    public static User CreateWithPassword(string email, string name, string passwordHash, Guid? personId, DateTime nowUtc)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = NormalizeEmail(email),
            Name = NormalizeName(name),
            PasswordHash = ValidateRequired(passwordHash, nameof(passwordHash)),
            AuthProvider = AuthProvider.Email,
            PersonId = personId,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };
    }

    public static User CreateWithGoogle(string email, string name, string googleSub, Guid? personId, DateTime nowUtc)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = NormalizeEmail(email),
            Name = NormalizeName(name),
            GoogleSub = ValidateRequired(googleSub, nameof(googleSub)),
            AuthProvider = AuthProvider.Google,
            PersonId = personId,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };
    }

    public void LinkGoogle(string googleSub, DateTime nowUtc)
    {
        GoogleSub = ValidateRequired(googleSub, nameof(googleSub));
        AuthProvider = AuthProvider.Google;
        Touch(nowUtc);
    }

    public void SetPasswordHash(string passwordHash, DateTime nowUtc)
    {
        PasswordHash = ValidateRequired(passwordHash, nameof(passwordHash));
        Touch(nowUtc);
    }

    public void UpdateName(string name, DateTime nowUtc)
    {
        Name = NormalizeName(name);
        Touch(nowUtc);
    }

    public void LinkPerson(Guid personId, DateTime nowUtc)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("PersonId invalido.", nameof(personId));

        PersonId = personId;
        Touch(nowUtc);
    }

    private static string NormalizeEmail(string email)
    {
        var value = ValidateRequired(email, nameof(email)).Trim().ToLowerInvariant();
        return value.Contains('@') ? value : throw new ArgumentException("Email invalido.", nameof(email));
    }

    private static string NormalizeName(string name)
    {
        var value = ValidateRequired(name, nameof(name)).Trim();
        return value.Length <= 200 ? value : throw new ArgumentException("Nome deve ter no maximo 200 caracteres.", nameof(name));
    }

    private static string ValidateRequired(string value, string paramName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{paramName} obrigatorio.", paramName)
            : value;
    }

    private void Touch(DateTime nowUtc)
    {
        UpdatedAt = nowUtc;
    }
}
