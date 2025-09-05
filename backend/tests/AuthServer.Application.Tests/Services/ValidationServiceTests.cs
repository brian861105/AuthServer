using NUnit.Framework;
using AuthServer.Application.Services;
using AuthServer.Domain.Interfaces;

namespace AuthServer.Application.Tests.Services;

[TestFixture]
public class ValidationServiceTests
{
    private IValidationService _validationService;

    [SetUp]
    public void Setup()
    {
        _validationService = new ValidationService();
    }

    [Test]
    public void ValidateEmail_WithValidEmail_ShouldReturnTrue()
    {
        // Act
        var result = _validationService.ValidateEmail("test@example.com");

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateEmail_WithInvalidEmail_ShouldReturnFalse()
    {
        // Act
        var result = _validationService.ValidateEmail("invalid-email");

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Error, Is.Not.Empty);
    }

    [Test]
    public void ValidatePassword_WithStrongPassword_ShouldReturnTrue()
    {
        // Act
        var result = _validationService.ValidatePassword("StrongPass123!");

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidatePassword_WithWeakPassword_ShouldReturnFalse()
    {
        // Act
        var result = _validationService.ValidatePassword("123");

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Error, Is.Not.Empty);
    }
}