using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class MembershipViewModel : ObservableObject
{
    private readonly ObservableCollection<Customer> _allCustomers = new();

    [ObservableProperty]
    private bool _isLoading;

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

    private readonly ICustomerService _customerService;

    public MembershipViewModel() : this(App.Services.GetRequiredService<ICustomerService>())
    {
    }

    public MembershipViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        _ = LoadCustomersAsync();
    }

    [RelayCommand]
    private void ToggleRegister()
    {
        ShowRegister = !ShowRegister;
        NewName = "";
        NewPhone = "";
    }

    private async Task LoadCustomersAsync()
    {
        IsLoading = true;
        try
        {
            // Increase PageSize to 100 for better local search coverage
            var result = await _customerService.GetAll(new MyShop.Data.PagingRequest { PageSize = 100 });
            _allCustomers.Clear();
            if (result?.Items != null)
            {
                foreach (var customer in result.Items)
                {
                    _allCustomers.Add(customer);
                }
            }
            OnSearchQueryChanged(SearchQuery);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectMember(Customer? customer)
    {
        MemberSelected?.Invoke(customer);
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrEmpty(NewName) || string.IsNullOrEmpty(NewPhone)) return;

        var newCust = new Customer
        {
            Name = NewName,
            Phone = NewPhone,
            TotalAccumulatedPoints = 0,
            CurrentAvailablePoints = 0,
            Tier = LoyaltyTier.Bronze,
            Status = "Active"
        };

        try
        {
            newCust = await _customerService.Add(newCust);
            _allCustomers.Insert(0, newCust);
            OnSearchQueryChanged(SearchQuery);
            MemberSelected?.Invoke(newCust);
            ToggleRegister();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilteredCustomers.Clear();
        
        string query = (value ?? "").Trim().ToLower();

        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var c in _allCustomers) FilteredCustomers.Add(c);
            OnPropertyChanged(nameof(FilteredCustomers));
            return;
        }

        foreach (var c in _allCustomers)
        {
            bool match = (c.Name?.ToLower()?.Contains(query) == true) ||
                         (c.Phone?.Contains(query) == true);
            
            if (match)
            {
                FilteredCustomers.Add(c);
            }
        }

        OnPropertyChanged(nameof(FilteredCustomers));
        System.Diagnostics.Debug.WriteLine($"Member Search: '{query}' found {FilteredCustomers.Count} items.");
    }
}
