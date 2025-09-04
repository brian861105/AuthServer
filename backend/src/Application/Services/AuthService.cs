using AuthServer.Domain.Common;
using AuthServer.Domain.Entities;
using AuthServer.Domain.Interfaces;
using BCrypt.Net;

namespace AuthServer.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(string email, string password)
    {
        if (await _userRepository.ExistsByEmailAsync(email))
        {
            return Result<AuthResponse>.Failure("Email already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User(email, passwordHash);

        await _userRepository.AddAsync(user);

        var token = GenerateSimpleToken(user);
        return Result<AuthResponse>.Success(new AuthResponse { Token = token });
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password)
    {
        Console.WriteLine($"[DEBUG] LoginAsync - Attempting login for: {email}");

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            Console.WriteLine($"[DEBUG] LoginAsync - User not found for email: {email}");
            return Result<AuthResponse>.Failure("Invalid email or password");
        }

        Console.WriteLine($"[DEBUG] LoginAsync - User found, verifying password...");
        Console.WriteLine($"[DEBUG] LoginAsync - Input password: {password}");
        Console.WriteLine($"[DEBUG] LoginAsync - Stored hash: {user.PasswordHash[..10]}...");

        var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        Console.WriteLine($"[DEBUG] LoginAsync - Password verification result: {passwordValid}");

        if (!passwordValid)
        {
            return Result<AuthResponse>.Failure("Invalid email or password");
        }

        var token = GenerateSimpleToken(user);
        return Result<AuthResponse>.Success(new AuthResponse { Token = token });
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            // 安全考量：即使用戶不存在也回傳成功，避免暴露用戶是否存在
            return Result.Success();
        }

        var resetToken = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddHours(1); // 1小時後過期

        user.SetResetToken(resetToken, expiry);
        await _userRepository.UpdateAsync(user);

        // 曳光彈階段：在此輸出重設連結，實際應該發送 Email
        Console.WriteLine($"Reset link: /reset-password?token={resetToken}");

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _userRepository.GetByResetTokenAsync(token);
        if (user == null || !user.IsResetTokenValid(token))
        {
            return Result.Failure("Invalid or expired reset token");
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatePassword(newPasswordHash);

        await _userRepository.UpdateAsync(user);

        return Result.Success();
    }

    private string GenerateSimpleToken(User user)
    {
        // 曳光彈階段：最簡單的 token 生成
        var payload = $"{user.Id}:{user.Email}:{DateTime.UtcNow.AddDays(1):O}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
    }
}