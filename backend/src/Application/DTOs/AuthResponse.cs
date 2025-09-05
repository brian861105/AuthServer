using Swashbuckle.AspNetCore.Annotations;

namespace AuthServer.Application.DTOs;

[SwaggerSchema("Authentication response containing JWT token")]
public class AuthResponse
{
    [SwaggerSchema("JWT Bearer token for authentication")]
    public string Token { get; set; } = string.Empty;
}