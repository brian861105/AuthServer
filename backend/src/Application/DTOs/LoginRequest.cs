using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs;

[SwaggerSchema("User login request")]
public class LoginRequest
{
    [SwaggerSchema("Registered email address")]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [SwaggerSchema("User password")]
    [Required]
    public string Password { get; set; } = string.Empty;
}