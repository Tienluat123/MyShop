using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using MyShop.ViewModels;

namespace MyShop.Views.Pos.Components;

public sealed partial class MembershipSelectionModal : ContentDialog
{
    public MembershipViewModel ViewModel { get; }

    public MembershipSelectionModal()
    {
        this.InitializeComponent();
        ViewModel = new MembershipViewModel();
        DataContext = ViewModel;

        SkipButton.Command = ViewModel.SelectMemberCommand;
        CreateSelectButton.Command = ViewModel.RegisterCommand;
        
        ViewModel.MemberSelected += (member) => this.Hide();
    }
}
