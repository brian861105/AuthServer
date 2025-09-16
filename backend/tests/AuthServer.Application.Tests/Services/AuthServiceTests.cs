using NUnit.Framework;
using Moq;
using AuthServer.Application.Services;
using AuthServer.Domain.Entities;
using AuthServer.Domain.Interfaces;
using AuthServer.Domain.Common;

namespace AuthServer.Application.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private AuthService _authService;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IValidationService> _mockValidationService;
    private Mock<ITokenService> _mockTokenService;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockValidationService = new Mock<IValidationService>();
        _mockTokenService = new Mock<ITokenService>();
        
        _authService = new AuthService(
            _mockUserRepository.Object, 
            _mockValidationService.Object, 
            _mockTokenService.Object
        );
    }

    #region RegisterAsync Tests
    
    [Test]
    public async Task RegisterAsync_WithValidData_ShouldReturnAuthResponseWithJwtToken()
    {
        // Arrange
        var email = "test@example.com";
        var password = "StrongPass123!";
        var expectedToken = "jwt.token.here";

        _mockValidationService.Setup(x => x.ValidateEmail(email))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(x => x.ValidatePassword(password))
            .Returns(ValidationResult.Success());
        _mockUserRepository.Setup(x => x.ExistsByEmailAsync(email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedToken);

        // Act
        var result = await _authService.RegisterAsync(email, password);

        // Assert
        Assert.That(result.Token, Is.EqualTo(expectedToken));
    }

    [Test]
    public async Task RegisterAsync_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var email = "invalid-email";
        var password = "StrongPass123!";

        _mockValidationService.Setup(x => x.ValidateEmail(email))
            .Returns(ValidationResult.Failure("Invalid email format"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(
            () => _authService.RegisterAsync(email, password));
        
        Assert.That(exception.Message, Is.EqualTo("Invalid email format (Parameter 'email')"));
        Assert.That(exception.ParamName, Is.EqualTo("email"));
    }

    [Test]
    public async Task RegisterAsync_WithWeakPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var email = "test@example.com";
        var password = "123";

        _mockValidationService.Setup(x => x.ValidateEmail(email))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(x => x.ValidatePassword(password))
            .Returns(ValidationResult.Failure("Password is too weak"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(
            () => _authService.RegisterAsync(email, password));
        
        Assert.That(exception.Message, Is.EqualTo("Password is too weak (Parameter 'password')"));
        Assert.That(exception.ParamName, Is.EqualTo("password"));
    }

    [Test]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "StrongPass123!";

        _mockValidationService.Setup(x => x.ValidateEmail(email))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(x => x.ValidatePassword(password))
            .Returns(ValidationResult.Success());
        _mockUserRepository.Setup(x => x.ExistsByEmailAsync(email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(email, password));
        
        Assert.That(exception.Message, Is.EqualTo("Email already exists"));
    }

    #endregion

    #region LoginAsync Tests

    [Test]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponseWithJwtToken()
    {
        // Arrange
        var email = "test@example.com";
        var password = "StrongPass123!";
        var expectedToken = "jwt.token.here";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("StrongPass123!");
        var user = new User(email, hashedPassword);

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _mockTokenService.Setup(x => x.GenerateToken(user))
            .Returns(expectedToken);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.That(result.Token, Is.EqualTo(expectedToken));
    }

    [Test]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "password";

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(email, password));
        
        Assert.That(exception.Message, Is.EqualTo("Invalid email or password"));
    }

    [Test]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var email = "test@example.com";
        var correctPassword = "CorrectPass123!";
        var wrongPassword = "WrongPass123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
        var user = new User(email, hashedPassword);

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(email, wrongPassword));
        
        Assert.That(exception.Message, Is.EqualTo("Invalid email or password"));
    }

    #endregion

    #region ForgotPasswordAsync Tests

    [Test]
    public async Task ForgotPasswordAsync_WithExistingUser_ShouldUpdateUserWithResetToken()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User(email, "hashedpassword");

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        await _authService.ForgotPasswordAsync(email);

        // Assert
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.Email == email && !string.IsNullOrEmpty(u.ResetToken))), Times.Once);
    }

    [Test]
    public async Task ForgotPasswordAsync_WithNonExistentUser_ShouldNotThrowException()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        Assert.DoesNotThrowAsync(() => _authService.ForgotPasswordAsync(email));
        
        // Verify that UpdateAsync is never called for non-existent users
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Test]
    public async Task ResetPasswordAsync_WithInvalidToken_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidToken = "invalid-token";
        var newPassword = "NewPass123!";

        _mockUserRepository.Setup(x => x.GetByResetTokenAsync(invalidToken))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(
            () => _authService.ResetPasswordAsync(invalidToken, newPassword));
        
        Assert.That(exception.Message, Is.EqualTo("Invalid or expired reset token"));
    }

    [Test]
    public async Task ResetPasswordAsync_WithValidToken_ShouldUpdatePassword()
    {
        // Arrange
        var validToken = "valid-reset-token";
        var newPassword = "NewStrongPass123!";
        var user = new User("test@example.com", "oldhashedpassword");
        user.SetResetToken(validToken, DateTime.UtcNow.AddHours(1)); // Valid, not expired

        _mockUserRepository.Setup(x => x.GetByResetTokenAsync(validToken))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        await _authService.ResetPasswordAsync(validToken, newPassword);

        // Assert
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.Email == "test@example.com" && u.ResetToken == null)), Times.Once);
    }

    #endregion
}