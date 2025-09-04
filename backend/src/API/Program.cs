using AuthServer.API.Configuration;
using AuthServer.Application.Services;
using AuthServer.Domain.Interfaces;
using AuthServer.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();