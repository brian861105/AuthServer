using AuthServer.Domain.Entities;
using AuthServer.Domain.Interfaces;
using BCrypt.Net;

namespace AuthServer.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidationService _validationService;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IValidationService validationService, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _validationService = validationService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(string email, string password)
    {
        // Validate email
        var emailValidation = _validationService.ValidateEmail(email);
        if (!emailValidation.IsValid)
        {
            throw new ArgumentException(emailValidation.Error, nameof(email));
        }

        // Validate password
        var passwordValidation = _validationService.ValidatePassword(password);
        if (!passwordValidation.IsValid)
        {
            throw new ArgumentException(passwordValidation.Error, nameof(password));
        }

        // Check if email already exists
        if (await _userRepository.ExistsByEmailAsync(email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User(email, passwordHash);

        await _userRepository.AddAsync(user);

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse { Token = token };
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        Console.WriteLine($"[DEBUG] LoginAsync - Attempting login for: {email}");

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            Console.WriteLine($"[DEBUG] LoginAsync - User not found for email: {email}");
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        Console.WriteLine($"[DEBUG] LoginAsync - User found, verifying password...");
        Console.WriteLine($"[DEBUG] LoginAsync - Input password: {password}");
        Console.WriteLine($"[DEBUG] LoginAsync - Stored hash: {user.PasswordHash[..10]}...");

        var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        Console.WriteLine($"[DEBUG] LoginAsync - Password verification result: {passwordValid}");

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse { Token = token };
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            // 安全考量：即使用戶不存在也回傳成功，避免暴露用戶是否存在
            return;
        }

        var resetToken = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddHours(1); // 1小時後過期

        user.SetResetToken(resetToken, expiry);
        await _userRepository.UpdateAsync(user);

        // 曳光彈階段：在此輸出重設連結，實際應該發送 Email
        Console.WriteLine($"Reset link: /reset-password?token={resetToken}");
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _userRepository.GetByResetTokenAsync(token);
        if (user == null || !user.IsResetTokenValid(token))
        {
            throw new ArgumentException("Invalid or expired reset token");
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatePassword(newPasswordHash);

        await _userRepository.UpdateAsync(user);
    }

}