using AuthServer.Domain.Common;

namespace AuthServer.Domain.Interfaces;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
}

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(string email, string password);
    Task<Result<AuthResponse>> LoginAsync(string email, string password);
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(string token, string newPassword);
}