using AuthServer.Domain.Entities;

namespace AuthServer.Domain.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    int? GetUserIdFromToken(string token);
}