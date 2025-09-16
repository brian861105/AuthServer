namespace AuthServer.Domain.Interfaces;

/// <summary>
/// Core authentication business logic
/// </summary>
public interface IAuthService
{
    Task<string> RegisterAsync(string email, string password);
    Task<string> LoginAsync(string email, string password);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
}