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

    [Test]
    public async Task RegisterAsync_WithValidData_ShouldReturnSuccessWithJwtToken()
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
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Token, Is.EqualTo(expectedToken));
    }

    [Test]
    public async Task RegisterAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = "invalid-email";
        var password = "StrongPass123!";

        _mockValidationService.Setup(x => x.ValidateEmail(email))
            .Returns(ValidationResult.Failure("Invalid email format"));

        // Act
        var result = await _authService.RegisterAsync(email, password);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("Invalid email format"));
    }

    [Test]
    public async Task RegisterAsync_WithWeakPassword_ShouldReturnFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "123";

        _mockValidationService.Setup(x => x.ValidateEmail(email))
            .Returns(ValidationResult.Success());
        _mockValidationService.Setup(x => x.ValidatePassword(password))
            .Returns(ValidationResult.Failure("Password is too weak"));

        // Act
        var result = await _authService.RegisterAsync(email, password);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("Password is too weak"));
    }

    [Test]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnJwtToken()
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
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Token, Is.EqualTo(expectedToken));
    }
}