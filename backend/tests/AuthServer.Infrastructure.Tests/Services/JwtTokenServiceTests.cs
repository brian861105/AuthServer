using NUnit.Framework;
using AuthServer.Domain.Entities;
using AuthServer.Domain.Interfaces;
using AuthServer.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AuthServer.Infrastructure.Tests.Services;

[TestFixture]
public class JwtTokenServiceTests
{
    private ITokenService _tokenService;
    private IConfiguration _configuration;
    private User _testUser;

    [SetUp]
    public void Setup()
    {
        var configData = new Dictionary<string, string>
        {
            ["JwtSettings:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long",
            ["JwtSettings:Issuer"] = "test-issuer",
            ["JwtSettings:Audience"] = "test-audience",
            ["JwtSettings:ExpirationMinutes"] = "60"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        _tokenService = new JwtTokenService(_configuration);
        _testUser = new User("test@example.com", "hashed-password");
    }

    [Test]
    public void GenerateToken_ShouldReturnValidJwtToken()
    {
        // Act
        var token = _tokenService.GenerateToken(_testUser);

        // Assert - 這個測試現在會失敗，因為還沒實作
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);
        Assert.That(token.Split('.'), Has.Length.EqualTo(3)); // JWT has 3 parts
    }
}