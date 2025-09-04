namespace AuthServer.API.Configuration;

public class DependencyConfiguration
{
    public UserRepositoryConfiguration UserRepository { get; set; } = new();
}

public class UserRepositoryConfiguration
{
    public string Lifetime { get; set; } = "Scoped";
    public string Comment { get; set; } = string.Empty;

    public ServiceLifetime GetServiceLifetime()
    {
        return Lifetime.ToLowerInvariant() switch
        {
            "singleton" => ServiceLifetime.Singleton,
            "transient" => ServiceLifetime.Transient,
            "scoped" => ServiceLifetime.Scoped,
            _ => ServiceLifetime.Scoped
        };
    }
}