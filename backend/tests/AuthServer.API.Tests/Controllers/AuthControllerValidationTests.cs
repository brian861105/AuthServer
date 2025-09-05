using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AuthServer.API.Tests.Controllers;

[TestFixture]
public class AuthControllerValidationTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task ValidateToken_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/validate-token");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ValidateToken_WithValidToken_ShouldReturnTokenInfo()
    {
        // Arrange - 註冊並登入取得有效 Token
        var loginRequest = new
        {
            Email = "test@example.com",
            Password = "TestPass123!"
        };

        // 先註冊用戶
        await _client.PostAsync("/api/auth/register",
            new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json"));

        // 登入取得 Token
        var loginResponse = await _client.PostAsync("/api/auth/login",
            new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json"));

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent);
        var token = loginResult.GetProperty("token").GetString();

        // 設定 JWT Token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/auth/validate-token");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.That(result.GetProperty("isValid").GetBoolean(), Is.True);
        Assert.That(result.GetProperty("email").GetString(), Is.EqualTo("test@example.com"));
        Assert.That(result.GetProperty("message").GetString(), Is.EqualTo("Token is valid"));
    }
}