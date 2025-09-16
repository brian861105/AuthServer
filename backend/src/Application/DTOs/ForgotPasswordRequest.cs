using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs;

[SwaggerSchema("Forgot password request")]
public class ForgotPasswordRequest
{
    [SwaggerSchema("Email address to send password reset link")]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}