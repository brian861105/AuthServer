using System.Text.RegularExpressions;
using AuthServer.Domain.Interfaces;

namespace AuthServer.Application.Services;

public class ValidationService : IValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public ValidationResult ValidateEmail(string email)
    {
        // 🔵 重構階段：真正的 Email 驗證邏輯
        if (string.IsNullOrWhiteSpace(email))
        {
            return ValidationResult.Failure("Email is required");
        }

        if (!EmailRegex.IsMatch(email))
        {
            return ValidationResult.Failure("Invalid email format");
        }

        return ValidationResult.Success();
    }

    public ValidationResult ValidatePassword(string password)
    {
        // 🔵 重構階段：真正的密碼強度驗證
        if (string.IsNullOrWhiteSpace(password))
        {
            return ValidationResult.Failure("Password is required");
        }

        if (password.Length < 8)
        {
            return ValidationResult.Failure("Password must be at least 8 characters long");
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        if (!hasUpper)
        {
            return ValidationResult.Failure("Password must contain at least one uppercase letter");
        }

        if (!hasLower)
        {
            return ValidationResult.Failure("Password must contain at least one lowercase letter");
        }

        if (!hasDigit)
        {
            return ValidationResult.Failure("Password must contain at least one digit");
        }

        if (!hasSpecial)
        {
            return ValidationResult.Failure("Password must contain at least one special character");
        }

        return ValidationResult.Success();
    }
}