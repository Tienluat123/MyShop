using System;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyShop.Data;
using MyShop.Di;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views;
using Uno.Resizetizer;

namespace MyShop;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Static provider to access services globally if needed (like in XAML)
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    protected Window? MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();
#if DEBUG
        MainWindow.UseStudio();
#endif

        // Setup Dependency Injection trước khi tạo UI
        RegisterServices();

        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (MainWindow.Content is not Frame rootFrame)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            // Place the frame in the current Window
            MainWindow.Content = rootFrame;

            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            // When the navigation stack isn't restored navigate to the first page,
            // configuring the new page by passing required information as a navigation
            // parameter
            rootFrame.Navigate(typeof(MainPage), args.Arguments);
        }

        MainWindow.SetWindowIcon();
        // Ensure the current window is active
        MainWindow.Activate();
    }

    /// <summary>
    /// Register tất cả services vào Dependency Injection Container
    /// Gọi trong OnLaunched trước khi initialize UI
    /// </summary>
    private void RegisterServices()
    {
        var container = ServiceContainer.Instance;
        var services = new ServiceCollection();

        // Create log file to track database initialization
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MyShop",
            "database.log"
        );
        Directory.CreateDirectory(Path.GetDirectoryName(logPath));

        void LogMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            try { File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n"); }
            catch { }
        }

        try
        {
            LogMessage("====== DATABASE INITIALIZATION START ======");

            // ===== Load Database Configuration from appsettings.json =====
            // Try multiple paths for configuration file
            var pathsToTry = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"), // Same dir as executable
                Path.Combine(AppContext.BaseDirectory, "..", "..", "appsettings.json"), // Project root (debug)
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyShop", "appsettings.json") // User data
            };

            DatabaseConfig dbConfig = null;
            string foundPath = null;

            foreach (var path in pathsToTry)
            {
                var fullPath = Path.GetFullPath(path);
                LogMessage($"🔍 Looking for config at: {fullPath}");

                if (File.Exists(fullPath))
                {
                    foundPath = fullPath;
                    break;
                }
            }

            if (foundPath != null)
            {
                try
                {
                    var json = File.ReadAllText(foundPath);
                    using (var doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        
                        // Try ConnectionStrings:DefaultConnection first (standard EF Core)
                        if (root.TryGetProperty("ConnectionStrings", out var connStrings) && 
                            connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                        {
                            var connStr = defaultConn.GetString();
                            LogMessage($"✅ Found connection string in ConnectionStrings:DefaultConnection");
                            dbConfig = ParseConnectionString(connStr);
                        }
                        // Fallback to custom Database section
                        else if (root.TryGetProperty("Database", out var dbElement))
                        {
                            dbConfig = new DatabaseConfig
                            {
                                Host = dbElement.TryGetProperty("Host", out var h) ? h.GetString() : "localhost",
                                Database = dbElement.TryGetProperty("Database", out var d) ? d.GetString() : "postgres",
                                Username = dbElement.TryGetProperty("Username", out var u) ? u.GetString() : "postgres",
                                Password = dbElement.TryGetProperty("Password", out var p) ? p.GetString() : "password",
                                Port = dbElement.TryGetProperty("Port", out var po) ? po.GetInt32() : 5432,
                                SslMode = dbElement.TryGetProperty("SslMode", out var s) ? s.GetString() : "Disable"
                            };
                            LogMessage($"✅ Loaded config from Database section");
                        }
                    }
                    
                    if (dbConfig != null) {
                        LogMessage($"✅ Config successful from: {foundPath}");
                        LogMessage($"   Host: {dbConfig.Host}:{dbConfig.Port}, DB: {dbConfig.Database}, User: {dbConfig.Username}");
                    } else {
                        LogMessage($"⚠️ Could not find supported config sections in {foundPath}. Using defaults.");
                        dbConfig = CreateDefaultConfig();
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"❌ Failed to parse appsettings.json from {foundPath}: {ex.Message}");
                    dbConfig = CreateDefaultConfig();
                }
            }
            else
            {
                LogMessage($"⚠️ appsettings.json not found in any location. Using default config.");
                dbConfig = CreateDefaultConfig();
            }

            // ===== Setup Database Context (PostgreSQL via Supabase) =====
            var connectionString = dbConfig.GetConnectionString();
            LogMessage($"📡 Database config loaded - Host: {dbConfig.Host}, Port: {dbConfig.Port}");

            AppDbContext dbContext = null;
            try
            {
                var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                    .UseNpgsql(connectionString, options =>
                    {
                        options.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorCodesToAdd: null
                        );
                    })
                    .Options;

                dbContext = new AppDbContext(dbContextOptions);

                // ✅ Test connection first
                LogMessage("🔍 Testing database connection...");
                var canConnect = dbContext.Database.CanConnect();
                if (canConnect)
                {
                    LogMessage("✅ Database connection successful!");

                    // ✅ Apply migrations (tạo database schema)
                    try
                    {
                        dbContext.Database.Migrate();
                        LogMessage("✅ Database migrations applied successfully");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"⚠️ Migration warning: {ex.Message}");
                        // Fallback: create schema if migrations fail
                        try
                        {
                            dbContext.Database.EnsureCreated();
                            LogMessage("✅ Database schema created (fallback)");
                        }
                        catch (Exception ex2) { LogMessage($"❌ Schema creation failed: {ex2.Message}"); }
                    }
                }
                else
                {
                    LogMessage("⚠️ Cannot connect to database - will retry on first access");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Database initialization error: {ex.Message}");
                LogMessage($"   Inner exception: {ex.InnerException?.Message}");
                LogMessage("   App will continue - ensure Supabase is accessible");

                // Create DbContext anyway - connection will be attempted on first use
                var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                    .UseNpgsql(connectionString, options =>
                    {
                        options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                    })
                    .Options;

                dbContext = new AppDbContext(dbContextOptions);
            }

            // ===== Register Data Access Layer (Repositories) =====
            container.Register<IRepository<Product>>(new Repository<Product>(dbContext));
            container.Register<IRepository<Category>>(new Repository<Category>(dbContext));
            container.Register<IRepository<Customer>>(new Repository<Customer>(dbContext));
            container.Register<IRepository<Order>>(new Repository<Order>(dbContext));
            container.Register<IRepository<OrderDetail>>(new Repository<OrderDetail>(dbContext));
            container.Register<IRepository<User>>(new Repository<User>(dbContext));
            container.Register<IRepository<Promotion>>(new Repository<Promotion>(dbContext));
            container.Register<IRepository<AppConfig>>(new Repository<AppConfig>(dbContext));
            container.Register<IRepository<UserSetting>>(new Repository<UserSetting>(dbContext));
            container.Register<IRepository<WorkSchedule>>(new Repository<WorkSchedule>(dbContext));

            services.AddSingleton<IRepository<Product>>(new Repository<Product>(dbContext));
            services.AddSingleton<IRepository<Category>>(new Repository<Category>(dbContext));
            services.AddSingleton<IRepository<Customer>>(new Repository<Customer>(dbContext));
            services.AddSingleton<IRepository<Order>>(new Repository<Order>(dbContext));
            services.AddSingleton<IRepository<OrderDetail>>(new Repository<OrderDetail>(dbContext));
            services.AddSingleton<IRepository<User>>(new Repository<User>(dbContext));
            services.AddSingleton<IRepository<Promotion>>(new Repository<Promotion>(dbContext));
            services.AddSingleton<IRepository<AppConfig>>(new Repository<AppConfig>(dbContext));
            services.AddSingleton<IRepository<UserSetting>>(new Repository<UserSetting>(dbContext));
            services.AddSingleton<IRepository<WorkSchedule>>(new Repository<WorkSchedule>(dbContext));

            ////Use InMemoryRepository for demo, or switch to Repository<T> for DB
            //services.AddSingleton<IRepository<Product>>(new InMemoryRepository<Product>());
            //services.AddSingleton<IRepository<Category>>(new InMemoryRepository<Category>());
            //services.AddSingleton<IRepository<Customer>>(new InMemoryRepository<Customer>());
            //services.AddSingleton<IRepository<Order>>(new InMemoryRepository<Order>());
            //services.AddSingleton<IRepository<OrderDetail>>(new InMemoryRepository<OrderDetail>());
            //services.AddSingleton<IRepository<User>>(new InMemoryRepository<User>());
            //services.AddSingleton<IRepository<Promotion>>(new InMemoryRepository<Promotion>());
            //services.AddSingleton<IRepository<AppConfig>>(new InMemoryRepository<AppConfig>());
            //services.AddSingleton<IRepository<UserSetting>>(new InMemoryRepository<UserSetting>());
            //services.AddSingleton<IRepository<WorkSchedule>>(new InMemoryRepository<WorkSchedule>());

            // ===== Register Business Logic Layer (Services) =====
            container.Register<IProductService>(new ProductService(
                container.Get<IRepository<Product>>(),
                container.Get<IRepository<Category>>()
            ));

            container.Register<ICustomerService>(new CustomerService(
                container.Get<IRepository<Customer>>()
            ));

            container.Register<IOrderService>(new OrderService(
                container.Get<IRepository<Order>>(),
                container.Get<IRepository<Customer>>(),
                container.Get<IRepository<User>>()
            ));

            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<ICustomerService, CustomerService>();
            services.AddSingleton<IOrderService, OrderService>();

            // ===== Register ViewModels =====
            services.AddTransient<OrderManagementViewModel>();
            // Add other viewmodels as needed...

            // Build Provider
            Services = services.BuildServiceProvider();

            //// Seed Mock Data
            //SampleDataSeeder.Seed(
            //    Services.GetRequiredService<IRepository<Product>>(),
            //    Services.GetRequiredService<IRepository<Category>>(),
            //    Services.GetRequiredService<IRepository<Order>>(),
            //    Services.GetRequiredService<IRepository<Customer>>(),
            //    Services.GetRequiredService<IRepository<User>>()
            //);

            LogMessage("✅ All services registered successfully with Microsoft DI");
            LogMessage("====== DATABASE INITIALIZATION END ======");
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Critical error during service registration: {ex.Message}");
            LogMessage($"   Inner exception: {ex.InnerException?.Message}");
            // App will continue - ensure to handle null services gracefully
        }
    }

    private DatabaseConfig ParseConnectionString(string connStr)
    {
        var config = new DatabaseConfig();
        var parts = connStr.Split(';');
        foreach (var part in parts)
        {
            var kv = part.Split('=');
            if (kv.Length != 2) continue;
            var key = kv[0].Trim().ToLower();
            var val = kv[1].Trim();

            switch (key)
            {
                case "host": config.Host = val; break;
                case "port": config.Port = int.TryParse(val, out int p) ? p : 5432; break;
                case "database": config.Database = val; break;
                case "username": 
                case "user id": config.Username = val; break;
                case "password": config.Password = val; break;
                case "ssl mode": config.SslMode = val; break;
            }
        }
        return config;
    }

    /// <summary>
    /// Tạo mặc định DatabaseConfig khi không đọc được appsettings.json
    /// </summary>
    private DatabaseConfig CreateDefaultConfig()
    {
        return new DatabaseConfig
        {
            Host = "db.YOUR_PROJECT_ID.supabase.co",
            Database = "postgres",
            Username = "postgres",
            Password = "your_password",
            Port = 5432,
            SslMode = "Require"
        };
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

    /// <summary>
    /// Configures global Uno Platform logging
    /// </summary>
    public static void InitializeLogging()
    {
#if DEBUG
        // Logging is disabled by default for release builds, as it incurs a significant
        // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
        // is a concern for your application, keep this disabled. If you're running on the web or
        // desktop targets, you can use URL or command line parameters to enable it.
        //
        // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

        var factory = LoggerFactory.Create(builder =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());

            // Log to the Visual Studio Debug console
            builder.AddConsole();
#else
            builder.AddConsole();
#endif

            // Exclude logs below this level
            builder.SetMinimumLevel(LogLevel.Information);

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);

            // Generic Xaml events
            // builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace );

            // Layouter specific messages
            // builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

            // builder.AddFilter("Windows.Storage", LogLevel.Debug );

            // Binding related messages
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );

            // Binder memory references tracking
            // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

            // DevServer and HotReload related
            // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

            // Debug JS interop
            // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
