using AuthServer.API.Configuration;
using AuthServer.Application.Services;
using AuthServer.Domain.Interfaces;
using AuthServer.Infrastructure.Data;
using AuthServer.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Configure dependency settings
var dependencyConfig = builder.Configuration.GetSection("Dependencies").Get<DependencyConfiguration>()
                      ?? new DependencyConfiguration();

// Register dependencies with configurable lifetime
var userRepoLifetime = dependencyConfig.UserRepository.GetServiceLifetime();

Console.WriteLine($"[CONFIG] UserRepository lifetime: {userRepoLifetime}");
Console.WriteLine($"[CONFIG] Comment: {dependencyConfig.UserRepository.Comment}");

switch (userRepoLifetime)
{
    case ServiceLifetime.Singleton:
        builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        break;
    case ServiceLifetime.Transient:
        builder.Services.AddTransient<IUserRepository, InMemoryUserRepository>();
        break;
    case ServiceLifetime.Scoped:
    default:
        builder.Services.AddScoped<IUserRepository, InMemoryUserRepository>();
        break;
}

// Register new services
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "default-secret-key-for-development-only";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "AuthServer",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "AuthServer",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// JWT Authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }