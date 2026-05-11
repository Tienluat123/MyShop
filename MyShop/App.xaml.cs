using System;
using Microsoft.Extensions.Logging;
using Uno.Resizetizer;
using MyShop.Views;
using MyShop.Data;
using MyShop.Services;
using MyShop.Di;
using MyShop.Models;
using MyShop.Services.ExcelImport;
using MyShop.ViewModels;
using MyShop.ViewModels.StaffManagement;
using Microsoft.EntityFrameworkCore;
using MyShop.Views.AppConfig;

namespace MyShop;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        this.InitializeComponent();
    }

    /// <summary>
    /// Static provider to access services globally if needed (like in XAML)
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    // Make MainWindow public for DialogService to access XamlRoot
    public Window? MainWindow { get; private set; }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();
#if DEBUG
        MainWindow.UseStudio();
#endif

        var configService = RegisterStartupConfigServices();

        var isDatabaseReady = await InitializeDatabaseConnection(configService);
        if (!isDatabaseReady)
        {
            ShowDatabaseConfigPage();
            MainWindow.SetWindowIcon();
            MainWindow.Activate();
            return;
        }

        RegisterServices(configService);

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            ShowLoginPage(args.Arguments);
        }

        MainWindow.SetWindowIcon();
        MainWindow.Activate();
    }

    private IConfigService RegisterStartupConfigServices()
    {
        var container = ServiceContainer.Instance;
        container.Clear();

        var configService = new ConfigService();
        container.Register<IConfigService>(configService);
        container.Register<DatabaseConfigViewModel>(new DatabaseConfigViewModel(configService));

        Services = container;
        return configService;
    }

    private async Task<bool> InitializeDatabaseConnection(IConfigService configService)
    {
        try
        {
            if (!configService.HasSavedConfig())
                return false;

            var testResult = await configService.TestSavedConfigAsync();
            return testResult.Success;
        }
        catch
        {
            return false;
        }
    }

    internal void ShowDatabaseConfigPage()
    {
        var page = new DatabaseConfigPage();
        page.ConfigurationSaved += DatabaseConfigPage_ConfigurationSaved;
        MainWindow!.Content = page;
    }

    private async void DatabaseConfigPage_ConfigurationSaved(object? sender, EventArgs e)
    {
        if (sender is DatabaseConfigPage page)
            page.ConfigurationSaved -= DatabaseConfigPage_ConfigurationSaved;

        var configService = ServiceContainer.Instance.Get<IConfigService>();
        var isReady = await InitializeDatabaseConnection(configService);
        if (!isReady)
            return;

        RegisterServices(configService);
        ShowLoginPage(string.Empty);
    }

    private void ShowLoginPage(string? arguments)
    {
        if (MainWindow!.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;
            MainWindow.Content = rootFrame;
        }

        if (rootFrame.Content?.GetType() != typeof(Views.LoginPage))
        {
            rootFrame.Navigate(typeof(Views.LoginPage), arguments ?? string.Empty);
        }
    }

    /// <summary>
    /// Register tất cả services vào Dependency Injection Container
    /// Gọi trong OnLaunched trước khi initialize UI
    /// </summary>
    private void RegisterServices(IConfigService? startupConfigService = null)
    {
        var container = ServiceContainer.Instance;
        container.Clear();

        try
        {
            var configService = startupConfigService ?? new ConfigService();
            container.Register<IConfigService>(configService);

            var dbConfig = configService.LoadAsync().GetAwaiter().GetResult();
            var connectionString = dbConfig.ToConnectionString();

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

                try
                {
                    dbContext.Database.Migrate();
                }
                catch (Exception)
                {
                    try
                    {
                        dbContext.Database.EnsureCreated();
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception)
            {
                var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                    .UseNpgsql(connectionString, options =>
                    {
                        options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                    })
                    .Options;

                dbContext = new AppDbContext(dbContextOptions);
            }

            var contextFactory = new AppDbContextFactory(configService);
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
            container.Register<IRepository<Expense>>(new Repository<Expense>(contextFactory));
            container.Register<IRepository<ProductImage>>(new Repository<ProductImage>(contextFactory));

            var dialogService = new DialogService();
            container.Register<IDialogService>(dialogService);

            var notificationService = new NotificationService();
            container.Register<INotificationService>(notificationService);

            var sessionService = new SessionService();
            container.Register<ISessionService>(sessionService);

            container.Register<IProductService>(new ProductService(
                container.Get<IRepository<Product>>(),
                container.Get<IRepository<Category>>(),
                dbContext
            ));

            container.Register<ICustomerService>(new CustomerService(
                container.Get<IRepository<Customer>>()
            ));

            container.Register<IOrderService>(new OrderService(
                container.Get<IRepository<Order>>(),
                container.Get<IRepository<Customer>>(),
                container.Get<IRepository<User>>(),
                dbContext
            ));

            container.Register<ICategoryService>(new CategoryService(
                container.Get<IRepository<Category>>()
            ));

            container.Register<IExcelCatalogImportService>(new ExcelCatalogImportService(contextFactory));

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

            container.Register<IAppConfigService>(new AppConfigService(
                container.Get<IRepository<AppConfig>>()
            ));

            container.Register<IUserSettingService>(new UserSettingService(
                container.Get<IRepository<UserSetting>>()
            ));

            var reportService = new ReportService(
                container.Get<IRepository<Order>>(),
                container.Get<IRepository<Product>>(),
                container.Get<IRepository<OrderDetail>>(),
                container.Get<IRepository<Expense>>()
            );
            container.Register<IReportService>(reportService);

            container.Register<IAuthService>(new AuthService(contextFactory));

            container.Register<WeeklyRosterViewModel>(new WeeklyRosterViewModel(
                container.Get<IScheduleService>(),
                container.Get<IEmployeeService>(),
                container.Get<INotificationService>()
            ));

            container.Register<PosViewModel>(new PosViewModel(
                container.Get<IProductService>(),
                container.Get<ICategoryService>(),
                container.Get<IOrderService>(),
                container.Get<IPromotionService>(),
                container.Get<ILoyaltyService>()
            ));

            container.Register<ConfigViewModel>(new ConfigViewModel(
                container.Get<IAppConfigService>()
            ));

            container.Register<DatabaseConfigViewModel>(new DatabaseConfigViewModel(
                container.Get<IConfigService>()
            ));

            container.Register<OrderManagementViewModel>(new OrderManagementViewModel(
                container.Get<IOrderService>(),
                container.Get<IProductService>()
            ));

            container.Register<EmployeeManagementViewModel>(new EmployeeManagementViewModel(
                container.Get<IEmployeeService>(),
                container.Get<IScheduleService>(),
                container.Get<INotificationService>(),
                container.Get<IAppConfigService>()
            ));
            container.Register<DashboardViewModel>(new DashboardViewModel(
    container.Get<IProductService>(),
    container.Get<IOrderService>()
));

            container.Register<EmployeeFormViewModel>(new EmployeeFormViewModel());

            container.Register<ReportViewModel>(new ReportViewModel(
                container.Get<IReportService>(),
                container.Get<ICategoryService>(),
                container.Get<IRepository<Expense>>(),
                container.Get<INotificationService>()
            ));

            Services = container;
        }
        catch (Exception)
        {
            throw;
        }
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
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
