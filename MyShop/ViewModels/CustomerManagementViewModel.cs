using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyShop.ViewModels;

public sealed class CustomerManagementViewModel : BaseViewModel
{
    private readonly List<CustomerListItem> allCustomers;
    private readonly Action onAdd;
    private readonly Action onRules;
    private readonly Action<CustomerListItem> onProfile;
    private readonly Action<CustomerListItem> onEdit;
    private string searchText = string.Empty;
    private CustomerListItem? selectedCustomer;

    public CustomerManagementViewModel(
        Action onAdd,
        Action onRules,
        Action<CustomerListItem> onProfile,
        Action<CustomerListItem> onEdit)
    {
        this.onAdd = onAdd;
        this.onRules = onRules;
        this.onProfile = onProfile;
        this.onEdit = onEdit;

        allCustomers = BuildSeedData();
        Customers = new ObservableCollection<CustomerListItem>(allCustomers);

        SearchCommand = new RelayCommand(ExecuteSearch);
        MapsToAddCommand = new RelayCommand(() => this.onAdd());
        MapsToRulesCommand = new RelayCommand(() => this.onRules());
        MapsToProfileCommand = new RelayCommand(OpenSelectedProfile, () => SelectedCustomer is not null);
    }

    public ObservableCollection<CustomerListItem> Customers { get; }

    public string SearchText
    {
        get => searchText;
        set
        {
            if (searchText != value)
            {
                searchText = value;
                OnPropertyChanged();
            }
        }
    }

    public CustomerListItem? SelectedCustomer
    {
        get => selectedCustomer;
        set
        {
            if (selectedCustomer != value)
            {
                selectedCustomer = value;
                OnPropertyChanged();
                MapsToProfileCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand MapsToAddCommand { get; }
    public RelayCommand MapsToRulesCommand { get; }
    public RelayCommand MapsToProfileCommand { get; }

    public void OpenProfile(CustomerListItem customer)
    {
        SelectedCustomer = customer;
        MapsToProfileCommand.Execute(null);
    }

    public void EditCustomer(CustomerListItem customer)
    {
        onEdit(customer);
    }

    private void OpenSelectedProfile()
    {
        if (SelectedCustomer is not null)
        {
            onProfile(SelectedCustomer);
        }
    }

    private void ExecuteSearch()
    {
        var query = SearchText?.Trim() ?? string.Empty;
        var result = string.IsNullOrWhiteSpace(query)
            ? allCustomers
            : allCustomers.Where(c =>
                c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                c.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        Customers.Clear();
        foreach (var customer in result)
        {
            Customers.Add(customer);
        }
    }

    private static List<CustomerListItem> BuildSeedData()
    {
        return
        [
            new CustomerListItem
            {
                Id = 1,
                Code = "CUST-001",
                Name = "Nguyen Van A",
                Initials = "NVA",
                Phone = "0901234567",
                Tier = "Gold",
                Points = 1250,
                Visits = 25,
            },
            new CustomerListItem
            {
                Id = 2,
                Code = "CUST-002",
                Name = "Tran Thi B",
                Initials = "TTB",
                Phone = "0912345678",
                Tier = "Silver",
                Points = 450,
                Visits = 8,
            },
            new CustomerListItem
            {
                Id = 3,
                Code = "CUST-003",
                Name = "Le Van C",
                Initials = "LVC",
                Phone = "0987654321",
                Tier = "Regular",
                Points = 0,
                Visits = 1,
            }
        ];
    }
}
