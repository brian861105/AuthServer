using AuthServer.Domain.Entities;

namespace AuthServer.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByResetTokenAsync(string token);
    Task<bool> ExistsByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
}