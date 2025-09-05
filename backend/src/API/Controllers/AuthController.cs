using AuthServer.Application.DTOs;
using AuthServer.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Authentication operations including user registration, login, and password management")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register a new user",
        Description = "Creates a new user account with email and password. Returns JWT token upon successful registration.",
        OperationId = "RegisterUser"
    )]
    [SwaggerResponse(200, "User registered successfully", typeof(AuthResponse))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(409, "Email already exists")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var token = await _authService.RegisterAsync(request.Email, request.Password);
        var response = new AuthResponse { Token = token };
        return Ok(response);
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Authenticate user",
        Description = "Authenticates a user with email and password. Returns JWT token upon successful authentication.",
        OperationId = "LoginUser"
    )]
    [SwaggerResponse(200, "User authenticated successfully", typeof(AuthResponse))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(401, "Invalid email or password")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        var response = new AuthResponse { Token = token };
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [SwaggerOperation(
        Summary = "Request password reset",
        Description = "Initiates password reset process for the specified email. Always returns success for security reasons (even if email doesn't exist).",
        OperationId = "ForgotPassword"
    )]
    [SwaggerResponse(200, "Password reset request processed")]
    [SwaggerResponse(400, "Invalid request data")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request.Email);
        return Ok(new { message = "Password reset link sent to your email" });
    }

    [HttpPost("reset-password")]
    [SwaggerOperation(
        Summary = "Reset user password",
        Description = "Resets user password using a valid reset token obtained from forgot-password endpoint.",
        OperationId = "ResetPassword"
    )]
    [SwaggerResponse(200, "Password reset successfully")]
    [SwaggerResponse(400, "Invalid or expired reset token")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
        return Ok(new { message = "Password reset successfully" });
    }

    [HttpGet("validate-token")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Validate JWT token",
        Description = "Validates the provided JWT token and returns user information if valid. Requires Bearer authentication.",
        OperationId = "ValidateToken"
    )]
    [SwaggerResponse(200, "Token is valid")]
    [SwaggerResponse(401, "Unauthorized - Invalid or missing token")]
    public IActionResult ValidateToken()
    {
        var userId = User.FindFirst("nameid")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new
        {
            isValid = true,
            userId = userId,
            email = email,
            message = "Token is valid"
        });
    }
}