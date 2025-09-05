namespace AuthServer.Domain.Interfaces;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
}

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(string email, string password);
    Task<AuthResponse> LoginAsync(string email, string password);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
}