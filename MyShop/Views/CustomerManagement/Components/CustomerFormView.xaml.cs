using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.ViewModels;

namespace MyShop.Views.CustomerManagement.Components;

public sealed partial class CustomerFormView : Page
{
    public CustomerFormView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as CustomerFormViewModel;
    }
}
