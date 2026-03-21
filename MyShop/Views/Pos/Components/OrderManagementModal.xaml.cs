using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using MyShop.ViewModels;
using MyShop.Models;

namespace MyShop.Views.Pos.Components;

public sealed partial class OrderManagementModal : ContentDialog
{
    public OrderManagementViewModel ViewModel;

    public OrderManagementModal(ObservableCollection<PosOrder> orders)
    {
        this.InitializeComponent();
        ViewModel = new OrderManagementViewModel(orders);
        DataContext = ViewModel;
    }

    private void CloseBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.Hide();
    }
}
