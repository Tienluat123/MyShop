using MyShop.Di;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.Pos;

namespace MyShop.Views;

public sealed partial class LoginPage : Page
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        this.InitializeComponent();

        var authService = ServiceContainer.Instance.Get<IAuthService>();
        var sessionService = ServiceContainer.Instance.Get<ISessionService>();
        _viewModel = new LoginViewModel(authService, sessionService);

        this.DataContext = _viewModel;

        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }


    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb)
        {
            _viewModel.Password = pb.Password;
        }
    }


    private void OnLoginSucceeded(string role)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            if (role == "Admin")
            {
                this.Frame?.Navigate(typeof(MainPage));
            }
            else
            {
                this.Frame?.Navigate(typeof(PosPage));
            }
        });
    }

    private void ConfigDatabase_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current is App app)
        {
            app.ShowDatabaseConfigPage();
        }
    }
}
