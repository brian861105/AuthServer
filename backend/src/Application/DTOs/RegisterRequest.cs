using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs;

[SwaggerSchema("User registration request")]
public class RegisterRequest
{
    [SwaggerSchema("Valid email address")]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [SwaggerSchema("Strong password (minimum 8 characters)")]
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}