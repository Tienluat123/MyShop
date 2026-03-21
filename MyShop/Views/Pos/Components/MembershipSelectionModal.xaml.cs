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

        SearchBox.SetBinding(TextBox.TextProperty, new Binding { Path = new PropertyPath("SearchQuery"), Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        SkipButton.Command = ViewModel.SelectMemberCommand;
        
        ViewModel.MemberSelected += (member) => this.Hide();
    }
}
