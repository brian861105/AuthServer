namespace AuthServer.Domain.Interfaces;

public interface IValidationService
{
    ValidationResult ValidateEmail(string email);
    ValidationResult ValidatePassword(string password);
}

public class ValidationResult
{
    public bool IsValid { get; }
    public string Error { get; }

    public ValidationResult(bool isValid, string error = "")
    {
        IsValid = isValid;
        Error = error;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string error) => new(false, error);
}