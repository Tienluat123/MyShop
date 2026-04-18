using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class CustomerProfileViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;
    private readonly Action _onBack;

    // 1. CHỈ KHAI BÁO BIẾN CÓ [ObservableProperty]. 
    // Thư viện sẽ tự tạo ra SelectedCustomer (viết hoa) cho bạn.
    [ObservableProperty]
    private CustomerDetail selectedCustomer = new();

    [ObservableProperty]
    private int activeTab;

    [ObservableProperty]
    private bool isLoading;

    public CustomerProfileViewModel(
        CustomerListItem selectedCustomerItem, 
        ICustomerService customerService, 
        IOrderService orderService, 
        Action onBack)
    {
        _customerService = customerService;
        _orderService = orderService;
        _onBack = onBack;

        // Gán giá trị vào Property viết hoa để kích hoạt thông báo UI
        SelectedCustomer.Customer = selectedCustomerItem;
        
      
    }
    [RelayCommand]
    public async Task LoadInitialData() // Đổi tên cho rõ nghĩa
    {
        await LoadAllData();
    }
    // 2. DÙNG [RelayCommand] cho nút Back để đồng bộ với bộ Toolkit
    [RelayCommand]
    private void GoBack() => _onBack?.Invoke();

    private async Task LoadAllData()
    {
        IsLoading = true;
        try
        {
            await Task.WhenAll(
                LoadProfileDetails(),
                LoadOrderHistory(),
                LoadLoyaltyPoints()
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadProfileDetails()
    {
        var dbCustomer = await Task.Run(() => _customerService.GetById(SelectedCustomer.Customer.Id));
        if (dbCustomer != null)
        {
            SelectedCustomer.Email = dbCustomer.Email ?? "N/A";
            SelectedCustomer.Address = dbCustomer.Address ?? "No address provided";
            SelectedCustomer.Birthday = dbCustomer.Birthday ?? DateTime.MinValue;
            SelectedCustomer.Notes = dbCustomer.Notes ?? "";
            SelectedCustomer.TotalSpent = (decimal)dbCustomer.TotalSpent;
            
            OnPropertyChanged(nameof(SelectedCustomer));
        }
    }

    [RelayCommand]
    private async Task LoadOrderHistory()
    {
        var orders = await Task.Run(() => _orderService.GetByCustomerId(SelectedCustomer.Customer.Id));
        Console.WriteLine($"[DEBUG] Khách hàng ID {SelectedCustomer.Customer.Id} có {orders.Count()} đơn hàng.");
        SelectedCustomer.Orders.Clear();
        foreach (var order in orders.OrderByDescending(o => o.OrderDate))
        {
            SelectedCustomer.Orders.Add(new OrderHistoryItem
            {
                OrderId = $"ORD-{order.Id}",
                Date = order.OrderDate,
                Amount = (decimal)order.TotalAmount,
                Items = "View in detail..."
            });
        }
    }

    [RelayCommand]
    private async Task LoadLoyaltyPoints()
    {
        SelectedCustomer.PointLogs.Clear();
        // Giả lập log điểm dựa trên đơn hàng hiện có
        foreach (var order in SelectedCustomer.Orders.Take(5))
        {
            SelectedCustomer.PointLogs.Add(new PointLogItem
            {
                Title = $"Earned from {order.OrderId}",
                Date = order.Date,
                DeltaPoints = (int)(order.Amount / 10)
            });
        }
    }

    [RelayCommand]
    private async void AddNote()
    {
        SelectedCustomer.Notes = "VIP customer. Prefers medium sugar and oat milk.";
        
        var dbCustomer = await _customerService.GetById(SelectedCustomer.Customer.Id);
        if (dbCustomer != null)
        {
            dbCustomer.Notes = SelectedCustomer.Notes;
            await _customerService.Update(dbCustomer);
        }
        
        OnPropertyChanged(nameof(SelectedCustomer));
    }
}