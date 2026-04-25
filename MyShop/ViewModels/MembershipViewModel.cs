using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;

namespace MyShop.ViewModels;

public partial class MembershipViewModel : ObservableObject
{
    private readonly ObservableCollection<Customer> _allCustomers = new();

    [ObservableProperty]
    private string _searchQuery = "";

    [ObservableProperty]
    private bool _showRegister;

    [ObservableProperty]
    private string _newName = "";

    [ObservableProperty]
    private string _newPhone = "";

    public ObservableCollection<Customer> FilteredCustomers { get; } = new();

    public event Action<Customer?>? MemberSelected;

    public MembershipViewModel()
    {
        LoadMockCustomers();
    }

    [RelayCommand]
    private void ToggleRegister()
    {
        ShowRegister = !ShowRegister;
        NewName = "";
        NewPhone = "";
    }

    private void LoadMockCustomers()
    {
        _allCustomers.Add(new Customer { Id = 1, Name = "Nguyen Van A", Phone = "0901234567", Points = 150 });
        _allCustomers.Add(new Customer { Id = 2, Name = "Tran Thi B", Phone = "0912345678", Points = 320 });
    }

    [RelayCommand]
    private void SelectMember(Customer? customer)
    {
        MemberSelected?.Invoke(customer);
    }

    [RelayCommand]
    private void Register()
    {
        if (string.IsNullOrEmpty(NewName) || string.IsNullOrEmpty(NewPhone)) return;

        var newCust = new Customer
        {
            Id = (int)DateTime.Now.Ticks,
            Name = NewName,
            Phone = NewPhone,
            Points = 0
        };
        MemberSelected?.Invoke(newCust);
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilteredCustomers.Clear();
        if (value.Length < 2) return;

        var filtered = _allCustomers.Where(c =>
            (c.Name != null && c.Name.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
            (c.Phone != null && c.Phone.Contains(value)));

        foreach (var c in filtered) FilteredCustomers.Add(c);
    }
}
