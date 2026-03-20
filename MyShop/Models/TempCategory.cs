using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class TempCategory : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string? _name;
}
