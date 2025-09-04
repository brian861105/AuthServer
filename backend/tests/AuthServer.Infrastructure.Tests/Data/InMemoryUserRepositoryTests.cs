using AuthServer.Domain.Entities;
using AuthServer.Infrastructure.Data;

namespace AuthServer.Infrastructure.Tests.Data;

[TestFixture]
public class InMemoryUserRepositoryTests
{
    private InMemoryUserRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _repository = new InMemoryUserRepository();
    }

    [Test]
    public async Task AddAsync_ShouldAssignUniqueId()
    {
        // Arrange
        var user = new User("test@example.com", "hashedPassword");

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.Email, Is.EqualTo("test@example.com"));
        Assert.That(result.PasswordHash, Is.EqualTo("hashedPassword"));
    }

    [Test]
    public async Task AddAsync_ShouldAssignIncrementingIds()
    {
        // Arrange
        var user1 = new User("user1@example.com", "hash1");
        var user2 = new User("user2@example.com", "hash2");

        // Act
        var result1 = await _repository.AddAsync(user1);
        var result2 = await _repository.AddAsync(user2);

        // Assert
        Assert.That(result1.Id, Is.EqualTo(1));
        Assert.That(result2.Id, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User("test@example.com", "hashedPassword");
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public async Task GetByEmailAsync_WhenUserNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User("test@example.com", "hashedPassword");
        var addedUser = await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(addedUser.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(addedUser.Id));
        Assert.That(result!.Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public async Task ExistsByEmailAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("test@example.com", "hashedPassword");
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.ExistsByEmailAsync("test@example.com");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsByEmailAsync_WhenUserNotExists_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateExistingUser()
    {
        // Arrange
        var user = new User("test@example.com", "originalHash");
        var addedUser = await _repository.AddAsync(user);
        
        addedUser.SetResetToken("resetToken", DateTime.UtcNow.AddHours(1));

        // Act
        await _repository.UpdateAsync(addedUser);
        var result = await _repository.GetByIdAsync(addedUser.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ResetToken, Is.EqualTo("resetToken"));
    }

    [Test]
    public async Task GetByResetTokenAsync_WhenTokenExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User("test@example.com", "hashedPassword");
        var addedUser = await _repository.AddAsync(user);
        
        addedUser.SetResetToken("resetToken123", DateTime.UtcNow.AddHours(1));
        await _repository.UpdateAsync(addedUser);

        // Act
        var result = await _repository.GetByResetTokenAsync("resetToken123");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("test@example.com"));
        Assert.That(result!.ResetToken, Is.EqualTo("resetToken123"));
    }
}