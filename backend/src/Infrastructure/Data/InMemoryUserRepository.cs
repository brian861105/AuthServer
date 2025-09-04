using AuthServer.Domain.Entities;
using AuthServer.Domain.Interfaces;

namespace AuthServer.Infrastructure.Data;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public Task<User?> GetByEmailAsync(string email)
    {
        Console.WriteLine($"[DEBUG] GetByEmailAsync - Looking for email: {email}");
        Console.WriteLine($"[DEBUG] GetByEmailAsync - Total users in memory: {_users.Count}");
        foreach (var u in _users)
        {
            Console.WriteLine($"[DEBUG] GetByEmailAsync - User: {u.Email}, PasswordHash: {u.PasswordHash[..10]}...");
        }
        
        var user = _users.FirstOrDefault(u => u.Email == email);
        Console.WriteLine($"[DEBUG] GetByEmailAsync - Found user: {user != null}");
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<User?> GetByResetTokenAsync(string token)
    {
        var user = _users.FirstOrDefault(u => u.ResetToken == token);
        return Task.FromResult(user);
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        var exists = _users.Any(u => u.Email == email);
        return Task.FromResult(exists);
    }

    public Task<User> AddAsync(User user)
    {
        Console.WriteLine($"[DEBUG] AddAsync - Adding user: {user.Email}");
        Console.WriteLine($"[DEBUG] AddAsync - PasswordHash: {user.PasswordHash[..10]}...");
        
        // 建立新的 User 實例，並設置 ID
        var newUser = new User(user.Email, user.PasswordHash);
        SetUserId(newUser, _nextId++);
        
        _users.Add(newUser);
        Console.WriteLine($"[DEBUG] AddAsync - Total users after add: {_users.Count}");
        
        return Task.FromResult(newUser);
    }

    public Task UpdateAsync(User user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            var index = _users.IndexOf(existingUser);
            _users[index] = user;
        }
        return Task.CompletedTask;
    }

    private void SetUserId(User user, int id)
    {
        // 使用反射設置 Id，因為 Id 的 setter 是 private
        var idProperty = typeof(User).GetProperty("Id");
        idProperty?.SetValue(user, id);
    }
}