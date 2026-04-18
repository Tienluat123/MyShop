using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views.AppConfig;

public sealed partial class AppConfigPage : Page
{
    public ConfigViewModel ViewModel { get; }

    public AppConfigPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ConfigViewModel>();
        DataContext = ViewModel;
    }
}
