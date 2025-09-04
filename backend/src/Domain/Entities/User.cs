namespace AuthServer.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? ResetToken { get; private set; }
    public DateTime? ResetTokenExpiry { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { } // 為 EF Core 保留

    public User(string email, string passwordHash)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        ClearResetToken();
    }

    public void SetResetToken(string token, DateTime expiry)
    {
        ResetToken = token;
        ResetTokenExpiry = expiry;
    }

    public void ClearResetToken()
    {
        ResetToken = null;
        ResetTokenExpiry = null;
    }

    public bool IsResetTokenValid(string token)
    {
        return ResetToken == token && 
               ResetTokenExpiry.HasValue && 
               ResetTokenExpiry > DateTime.UtcNow;
    }
}