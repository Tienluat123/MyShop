using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Uno.Resizetizer;
using MyShop.Views;
using MyShop.Data;
using MyShop.Services;
using MyShop.Di;
using MyShop.Models;
using MyShop.ViewModels;
using MyShop.ViewModels.StaffManagement;
using Microsoft.EntityFrameworkCore;

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

    // ✅ Make MainWindow public for DialogService to access XamlRoot
    public Window? MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();
#if DEBUG
        MainWindow.UseStudio();
#endif

        // Setup Dependency Injection trước khi tạo UI
        RegisterServices();

        // DialogService ready - XamlRoot được set tự động từ constructor

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
        // Cho phép Npgsql xử lý DateTime tự do không bắt buộc UTC
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var container = ServiceContainer.Instance;

        // Create log file to track database initialization
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MyShop",
            "database.log"
        );
        var logDirectory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

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

            DatabaseConfig? dbConfig = null;
            string? foundPath = null;

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
                        var dbElement = doc.RootElement.GetProperty("Database");
                        dbConfig = new DatabaseConfig
                        {
                            Host = dbElement.GetProperty("Host").GetString() ?? "localhost",
                            Database = dbElement.GetProperty("Database").GetString() ?? "postgres",
                            Username = dbElement.GetProperty("Username").GetString() ?? "postgres",
                            Password = dbElement.GetProperty("Password").GetString() ?? "password",
                            Port = dbElement.GetProperty("Port").GetInt32(),
                            SslMode = dbElement.GetProperty("SslMode").GetString() ?? "Require"
                        };
                    }
                    LogMessage($" Loaded config from: {foundPath}");
                    LogMessage($"   Host: {dbConfig.Host}:{dbConfig.Port}, DB: {dbConfig.Database}, User: {dbConfig.Username}");
                }
                catch (Exception ex)
                {
                    LogMessage($" Failed to parse appsettings.json from {foundPath}: {ex.Message}");
                    dbConfig = CreateDefaultConfig();
                }
            }
            else
            {
                LogMessage($"⚠️ appsettings.json not found in any location. Using default config.");
                dbConfig = CreateDefaultConfig();
            }

            // ===== Setup Database Context (PostgreSQL via Supabase) =====
            if (dbConfig is null)
            {
                dbConfig = CreateDefaultConfig();
            }

            var connectionString = dbConfig.GetConnectionString();
            LogMessage($"📡 Database config loaded - Host: {dbConfig.Host}, Port: {dbConfig.Port}");

            AppDbContext dbContext;
            try
            {
                var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                    .UseNpgsql(connectionString, options =>
                    {
                        options.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: new[] { "XX000", "ECIRCUITBREAKER", "EMAXCONNSESSION" }
                        );
                    })
                    .Options;

                dbContext = new AppDbContext(dbContextOptions);

                //  Skip synchronous CanConnect() - let migrations handle connection validation
                LogMessage("🔍 Database context initialized (validation deferred to migrations)");

                //  Apply migrations (tạo database schema)
                try
                {
                    dbContext.Database.Migrate();
                    LogMessage(" Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    LogMessage($"⚠️ Migration warning: {ex.Message}");
                    // Fallback: create schema if migrations fail
                    try
                    {
                        dbContext.Database.EnsureCreated();
                        LogMessage("Database schema created (fallback)");
                    }
                    catch (Exception ex2) { LogMessage($"Schema creation failed: {ex2.Message}"); }
                }
            }
            catch (Exception ex)
            {
                LogMessage($" Database initialization error: {ex.Message}");
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
            // Each Repository operation gets fresh DbContext instance
            var contextFactory = new AppDbContextFactory();
            container.Register<AppDbContextFactory>(contextFactory);

            container.Register<IRepository<Product>>(new Repository<Product>(contextFactory));
            container.Register<IRepository<Category>>(new Repository<Category>(contextFactory));
            container.Register<IRepository<Customer>>(new Repository<Customer>(contextFactory));
            container.Register<IRepository<Order>>(new Repository<Order>(contextFactory));
            container.Register<IRepository<OrderDetail>>(new Repository<OrderDetail>(contextFactory));
            container.Register<IRepository<User>>(new Repository<User>(contextFactory));
            container.Register<IRepository<Promotion>>(new Repository<Promotion>(contextFactory));
            container.Register<IRepository<AppConfig>>(new Repository<AppConfig>(contextFactory));
            container.Register<IRepository<UserSetting>>(new Repository<UserSetting>(contextFactory));
            container.Register<IRepository<WorkSchedule>>(new Repository<WorkSchedule>(contextFactory));
            container.Register<IRepository<LoyaltyTierConfig>>(new Repository<LoyaltyTierConfig>(contextFactory));
            container.Register<IRepository<ProductImage>>(new Repository<ProductImage>(contextFactory));

            // ===== Register Business Logic Layer (Services) =====
            var dialogService = new DialogService();
            container.Register<IDialogService>(dialogService);

            var notificationService = new NotificationService();
            container.Register<INotificationService>(notificationService);

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

            container.Register<ICategoryService>(new CategoryService(
                container.Get<IRepository<Category>>()
            ));

            container.Register<IScheduleService>(new ScheduleService(
                container.Get<IRepository<WorkSchedule>>()
            ));

            container.Register<IEmployeeService>(new EmployeeService(
                container.Get<IRepository<User>>(),
                contextFactory
            ));

            container.Register<ILoyaltyService>(new LoyaltyService(
                contextFactory.CreateDbContext(Array.Empty<string>()),
                container.Get<IRepository<Customer>>()
            ));

            container.Register<IPromotionRepository>(new PromotionRepository(
                contextFactory.CreateDbContext(Array.Empty<string>())
            ));

            container.Register<IPromotionService>(new PromotionService(
                container.Get<IPromotionRepository>()
            ));

            // AppConfigService — singleton, caches key-value settings in memory
            container.Register<IAppConfigService>(new AppConfigService(
                container.Get<IRepository<AppConfig>>()
            ));

            // WeeklyRosterViewModel requires both ScheduleService and EmployeeService
            container.Register<WeeklyRosterViewModel>(new WeeklyRosterViewModel(
                container.Get<IScheduleService>(),
                container.Get<IEmployeeService>(),
                container.Get<INotificationService>()
            ));

            // ===== Register ViewModels =====
            container.Register<PosViewModel>(new PosViewModel(
                container.Get<IProductService>(),
                container.Get<ICategoryService>(),
                container.Get<IOrderService>(),
                container.Get<IPromotionService>()
            ));

            container.Register<ConfigViewModel>(new ConfigViewModel(
                container.Get<IAppConfigService>()
            ));

            container.Register<OrderManagementViewModel>(new OrderManagementViewModel(
                container.Get<IOrderService>(),
                container.Get<IProductService>()
            ));

            container.Register<EmployeeManagementViewModel>(new EmployeeManagementViewModel(
                container.Get<IEmployeeService>(),
                container.Get<IScheduleService>(),
                container.Get<INotificationService>()
            ));

            container.Register<EmployeeFormViewModel>(new EmployeeFormViewModel());

            LogMessage(" All services registered successfully");
            // ===== Set static Services property for app-wide access =====
            Services = container;


            LogMessage(" All services registered successfully");
            LogMessage("====== DATABASE INITIALIZATION END ======");
        }
        catch (Exception ex)
        {
            LogMessage($" ERROR registering services: {ex.Message}");
            LogMessage($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Tạo mặc định DatabaseConfig khi không đọc được appsettings.json
    /// </summary>
    private DatabaseConfig CreateDefaultConfig()
    {
        return new DatabaseConfig
        {
            Host = "localhost",
            Database = "myshop",
            Username = "postgres",
            Password = "mpa113dn",
            Port = 5432,
            SslMode = "Disable"
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
