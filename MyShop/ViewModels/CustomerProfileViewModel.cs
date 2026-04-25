using System;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public sealed class CustomerProfileViewModel : BaseViewModel
{
    private readonly Action onBack;
    private int activeTab;

    public CustomerProfileViewModel(CustomerListItem selectedCustomer, Action onBack)
    {
        this.onBack = onBack;

        SelectedCustomer = new CustomerDetail
        {
            Customer = selectedCustomer,
            Email = "vana@example.com",
            Birthday = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Local),
            Address = "123 Le Loi, District 1, HCMC",
            TotalSpent = 450.50m,
            Notes = string.Empty,
            Orders =
            [
                new OrderHistoryItem { OrderId = "ORD-9921", Date = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Local), Items = "Caramel Latte, Croissant", Amount = 9.25m },
                new OrderHistoryItem { OrderId = "ORD-9845", Date = new DateTime(2024, 2, 25, 0, 0, 0, DateTimeKind.Local), Items = "Matcha Latte, Blueberry Muffin", Amount = 8.50m },
                new OrderHistoryItem { OrderId = "ORD-9712", Date = new DateTime(2024, 2, 18, 0, 0, 0, DateTimeKind.Local), Items = "Flat White", Amount = 4.50m }
            ],
            PointLogs =
            [
                new PointLogItem { Title = "Order #ORD-9921", Date = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Local), DeltaPoints = 50 },
                new PointLogItem { Title = "Redeemed for Discount", Date = new DateTime(2024, 2, 28, 0, 0, 0, DateTimeKind.Local), DeltaPoints = -200 },
                new PointLogItem { Title = "Order #ORD-9845", Date = new DateTime(2024, 2, 25, 0, 0, 0, DateTimeKind.Local), DeltaPoints = 45 }
            ]
        };

        AddNoteCommand = new RelayCommand(AddNote);
        RefreshDataCommand = new RelayCommand(() => { });
        BackCommand = new RelayCommand(() => this.onBack());
    }

    public CustomerDetail SelectedCustomer { get; }

    public int ActiveTab
    {
        get => activeTab;
        set
        {
            if (activeTab != value)
            {
                activeTab = value;
                OnPropertyChanged();
            }
        }
    }

    public RelayCommand AddNoteCommand { get; }
    public RelayCommand RefreshDataCommand { get; }
    public RelayCommand BackCommand { get; }

    private void AddNote()
    {
        if (string.IsNullOrWhiteSpace(SelectedCustomer.Notes))
        {
            SelectedCustomer.Notes = "VIP customer. Prefers medium sugar and oat milk.";
            OnPropertyChanged(nameof(SelectedCustomer));
        }
    }
}
