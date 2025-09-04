using AuthServer.Domain.Entities;
using AuthServer.Domain.Interfaces;
using AuthServer.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AuthServer.Infrastructure.Tests.DependencyInjection;

[TestFixture]
public class ServiceLifetimeTests
{
    [Test]
    public async Task InMemoryUserRepository_WithScopedLifetime_ShouldNotPersistDataBetweenScopes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        
        var serviceProvider = services.BuildServiceProvider();
        var testUser = new User("test@example.com", "hashedPassword");
        
        // Act & Assert - First scope: Add user
        using (var scope1 = serviceProvider.CreateScope())
        {
            var repository1 = scope1.ServiceProvider.GetRequiredService<IUserRepository>();
            await repository1.AddAsync(testUser);
            
            // Should find user in same scope
            var foundInSameScope = await repository1.GetByEmailAsync("test@example.com");
            Assert.That(foundInSameScope, Is.Not.Null);
        }
        
        // Act & Assert - Second scope: User should be gone
        using (var scope2 = serviceProvider.CreateScope())
        {
            var repository2 = scope2.ServiceProvider.GetRequiredService<IUserRepository>();
            
            // Should NOT find user in different scope - this is the bug we experienced
            var foundInDifferentScope = await repository2.GetByEmailAsync("test@example.com");
            Assert.That(foundInDifferentScope, Is.Null);
            
            // Also check ExistsByEmailAsync
            var existsInDifferentScope = await repository2.ExistsByEmailAsync("test@example.com");
            Assert.That(existsInDifferentScope, Is.False);
        }
    }
    
    [Test]
    public async Task InMemoryUserRepository_WithSingletonLifetime_ShouldPersistDataBetweenScopes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        
        var serviceProvider = services.BuildServiceProvider();
        var testUser = new User("singleton@example.com", "hashedPassword");
        
        // Act & Assert - First scope: Add user
        using (var scope1 = serviceProvider.CreateScope())
        {
            var repository1 = scope1.ServiceProvider.GetRequiredService<IUserRepository>();
            await repository1.AddAsync(testUser);
            
            // Should find user in same scope
            var foundInSameScope = await repository1.GetByEmailAsync("singleton@example.com");
            Assert.That(foundInSameScope, Is.Not.Null);
        }
        
        // Act & Assert - Second scope: User should still exist
        using (var scope2 = serviceProvider.CreateScope())
        {
            var repository2 = scope2.ServiceProvider.GetRequiredService<IUserRepository>();
            
            // Should find user in different scope - this is the fix
            var foundInDifferentScope = await repository2.GetByEmailAsync("singleton@example.com");
            Assert.That(foundInDifferentScope, Is.Not.Null);
            Assert.That(foundInDifferentScope.Email, Is.EqualTo("singleton@example.com"));
            
            // Also check ExistsByEmailAsync
            var existsInDifferentScope = await repository2.ExistsByEmailAsync("singleton@example.com");
            Assert.That(existsInDifferentScope, Is.True);
        }
    }
    
    [Test]
    public async Task InMemoryUserRepository_WithSingletonLifetime_ShouldAllowLoginAfterRegistration()
    {
        // Arrange - Simulate the exact bug scenario we experienced
        var services = new ServiceCollection();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        
        var serviceProvider = services.BuildServiceProvider();
        const string email = "integration@example.com";
        const string password = "testPassword";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        
        // Act & Assert - Scope 1: Registration (like HTTP POST /register)
        using (var registrationScope = serviceProvider.CreateScope())
        {
            var repository = registrationScope.ServiceProvider.GetRequiredService<IUserRepository>();
            var user = new User(email, passwordHash);
            
            await repository.AddAsync(user);
            
            // Verify registration worked in same scope
            var exists = await repository.ExistsByEmailAsync(email);
            Assert.That(exists, Is.True);
        }
        
        // Act & Assert - Scope 2: Login (like HTTP POST /login)
        using (var loginScope = serviceProvider.CreateScope())
        {
            var repository = loginScope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            // This is what failed in our original bug
            var user = await repository.GetByEmailAsync(email);
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Email, Is.EqualTo(email));
            
            // Verify password can be checked
            var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            Assert.That(passwordValid, Is.True);
        }
    }
    
    [TestCase("Singleton")]
    [TestCase("singleton")]
    [TestCase("SINGLETON")]
    public void UserRepositoryConfiguration_GetServiceLifetime_ShouldHandleSingletonCaseInsensitive(string lifetimeValue)
    {
        // Arrange
        var config = new AuthServer.API.Configuration.UserRepositoryConfiguration
        {
            Lifetime = lifetimeValue
        };
        
        // Act
        var lifetime = config.GetServiceLifetime();
        
        // Assert
        Assert.That(lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }
    
    [TestCase("Scoped")]
    [TestCase("scoped")]
    [TestCase("SCOPED")]
    [TestCase("")]
    [TestCase("invalid")]
    public void UserRepositoryConfiguration_GetServiceLifetime_ShouldDefaultToScoped(string lifetimeValue)
    {
        // Arrange
        var config = new AuthServer.API.Configuration.UserRepositoryConfiguration
        {
            Lifetime = lifetimeValue
        };
        
        // Act
        var lifetime = config.GetServiceLifetime();
        
        // Assert
        Assert.That(lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }
}