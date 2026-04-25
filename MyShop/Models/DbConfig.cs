namespace MyShop.Models;

public class DbConfig
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string DatabaseName { get; set; } = "myshop";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = string.Empty;

    public string ToConnectionString()
    {
        return $"Host={Server};Port={Port};Database={DatabaseName};Username={Username};Password={Password};" +
               "SslMode=Prefer;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=8;Minimum Pool Size=0;Connection Idle Lifetime=30;Command Timeout=30;";
    }
}
