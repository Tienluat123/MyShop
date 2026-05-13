using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views.Pos.Components;

public sealed partial class MembershipSelectionModal : ContentDialog
{
    public MembershipViewModel ViewModel { get; }
    public bool WasCancelled { get; private set; }

    public MembershipSelectionModal()
    {
        this.InitializeComponent();
        ViewModel = new MembershipViewModel();
        DataContext = ViewModel;

        SkipButton.Command = ViewModel.SelectMemberCommand;
        CreateSelectButton.Command = ViewModel.RegisterCommand;
        
        ViewModel.MemberSelected += (member) => this.Hide();
    }

    private void CancelCheckout_Click(object sender, RoutedEventArgs e)
    {
        WasCancelled = true;
        Hide();
    }
}
