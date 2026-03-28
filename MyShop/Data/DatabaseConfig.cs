namespace MyShop.Data;

/// <summary>
/// Database configuration class for Supabase PostgreSQL
/// </summary>
public class DatabaseConfig
{
    public string Host { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Port { get; set; } = 5432;
    public string SslMode { get; set; } = "Require";

    /// <summary>
    /// Build PostgreSQL connection string for Npgsql with Supabase support
    /// </summary>
    public string GetConnectionString()
    {
        // Connection pooler format for Supabase
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};" +
               $"SSL Mode={SslMode};" +
               $"Trust Server Certificate=false;" +
               $"Connection Idle Lifetime=300;" +
               $"Pooling=true;";
    }
}
