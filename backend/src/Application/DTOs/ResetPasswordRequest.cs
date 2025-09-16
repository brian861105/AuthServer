using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs;

[SwaggerSchema("Reset password request")]
public class ResetPasswordRequest
{
    [SwaggerSchema("Password reset token received via email")]
    [Required]
    public string Token { get; set; } = string.Empty;

    [SwaggerSchema("New password (minimum 8 characters)")]
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}