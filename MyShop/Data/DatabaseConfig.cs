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
        // Connection string tối ưu cho Supabase
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};" +
               $"SslMode={SslMode};" +
               $"Pooling=true;" +
               $"Maximum Pool Size=8;" +
               $"Minimum Pool Size=0;" +
               $"Connection Idle Lifetime=30;" +
               $"Command Timeout=30;";
    }
}
