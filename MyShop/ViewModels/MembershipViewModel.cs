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
    [NotifyPropertyChangedFor(nameof(ShowSearchView))]
    private bool _showRegister;

    public bool ShowSearchView => !ShowRegister && !ShowRedeemView;

    [ObservableProperty]
    private string _newName = "";

    [ObservableProperty]
    private string _newPhone = "";

    [ObservableProperty]
    private Customer? _selectedMember;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSearchView))]
    private bool _showRedeemView;

    [ObservableProperty]
    private int _pointsToRedeem;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private int _redemptionRate = 100; // Default if not loaded yet

    public ObservableCollection<Customer> FilteredCustomers { get; } = new();

    public event Action<Customer?>? MemberSelected;

    private readonly ICustomerService _customerService;
    private readonly ILoyaltyService _loyaltyService;

    public MembershipViewModel() : this(
        App.Services.GetRequiredService<ICustomerService>(),
        App.Services.GetRequiredService<ILoyaltyService>())
    {
    }

    public MembershipViewModel(ICustomerService customerService, ILoyaltyService loyaltyService)
    {
        _customerService = customerService;
        _loyaltyService = loyaltyService;
        _ = LoadCustomersAsync();
        _ = LoadRedemptionRate();
    }

    [RelayCommand]
    private void ToggleRegister()
    {
        ShowRegister = !ShowRegister;
        if (ShowRegister) ShowRedeemView = false;
        NewName = "";
        NewPhone = "";
        ErrorMessage = "";
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
                    // Hide guest/anonymous accounts
                    if (customer.Id > 0)
                    {
                        _allCustomers.Add(customer);
                    }
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
        if (customer == null)
        {
            MemberSelected?.Invoke(null);
            return;
        }

        SelectedMember = customer;
        PointsToRedeem = 0;
        DiscountAmount = 0;
        ShowRedeemView = true;
    }

    [RelayCommand]
    private void ConfirmSelection()
    {
        // This is called when user wants to just select the member WITHOUT redeeming points
        MemberSelected?.Invoke(SelectedMember);
    }

    [RelayCommand]
    private void ConfirmRedemption()
    {
        // Validation happens in UI or here
        if (PointsToRedeem > SelectedMember?.CurrentAvailablePoints)
        {
            // Error handling could be added here
            return;
        }
        
        // We pass the selection back. The actual deduction happens at checkout Finalize
        // But we can pass the intended points to use via a property or event
        MemberSelected?.Invoke(SelectedMember);
    }

    private async Task LoadRedemptionRate()
    {
        RedemptionRate = await _loyaltyService.GetRedemptionRateAsync();
    }

    partial void OnPointsToRedeemChanged(int value)
    {
        DiscountAmount = value * RedemptionRate;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = "";

        var name = NewName.Trim();
        var phone = NewPhone.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
        {
            ErrorMessage = "Name and phone number are required.";
            return;
        }

        var newCust = new Customer
        {
            Name = name,
            Phone = phone,
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
            ErrorMessage = ex.Message;
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
