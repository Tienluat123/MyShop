using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks; // Cần thiết cho Task
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models; 
using MyShop.Services;
using MyShop.Data; 
namespace MyShop.ViewModels;

public partial class CustomerManagementViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private const int PageSize = 10;

    private readonly Action onAdd;
    private readonly Action onRules;
    private readonly Action<CustomerListItem> onProfile;
    private readonly Action<CustomerListItem> onEdit;

    public ObservableCollection<CustomerListItem> DisplayCustomers { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private CustomerListItem? selectedCustomer;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalItems;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string pagingSummary = "Showing 0-0 in 0 items";

    public CustomerManagementViewModel(
        ICustomerService customerService,
        Action onAdd,
        Action onRules,
        Action<CustomerListItem> onProfile,
        Action<CustomerListItem> onEdit)
    {
        _customerService = customerService; // Phải gán giá trị này thì mới dùng được service
        this.onAdd = onAdd;
        this.onRules = onRules;
        this.onProfile = onProfile;
        this.onEdit = onEdit;

        // Gọi lệnh load lần đầu
        LoadCustomersCommand.Execute(null);
    }

    // --- LOGIC CỐT LÕI ---

    [RelayCommand]
private async Task LoadCustomers()
{
    var request = new PagingRequest 
    { 
        PageNumber = CurrentPage, 
        PageSize = PageSize,
        SearchKeyword = SearchText?.Trim() 
    };

    //var result = await Task.Run(() => _customerService.GetAll(request));
    var result = _customerService.GetAll(request);
    Console.WriteLine($"[DEBUG] Đã lấy được {result.Items.Count} khách hàng từ Database!");

    DisplayCustomers.Clear();
    foreach (var c in result.Items)
    {
        DisplayCustomers.Add(new CustomerListItem 
        { 
            Id = c.Id, 
            Name = c.Name, 
            Phone = c.Phone, 
            Code = $"CUST-{c.Id:D3}",
            Tier = "Active", 
            Initials = string.Join("", c.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x[0])) 
        });
    }

    // SỬA 2 DÒNG NÀY: Truy cập thông qua result.Pagination
    TotalItems = result.Pagination.TotalItems; 
    TotalPages = result.Pagination.TotalPages; 
    
    UpdatePagingSummary(result.Items.Count);
    UpdatePageNumbers();
}

    // --- HÀM BỔ TRỢ (Giải quyết lỗi thiếu hàm) ---

    private void UpdatePagingSummary(int currentItemsCount)
    {
        var start = TotalItems == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
        var end = TotalItems == 0 ? 0 : start + currentItemsCount - 1;
        PagingSummary = $"Showing {start}-{end} in {TotalItems} customers";
    }

    private void UpdatePageNumbers()
    {
        PageNumbers.Clear();
        for (int i = 1; i <= TotalPages; i++) PageNumbers.Add(i);
    }

    // --- COMMANDS ---

    [RelayCommand]
    private void Search()
    {
        CurrentPage = 1;
        LoadCustomersCommand.Execute(null);
    }

    partial void OnSearchTextChanged(string value) => Search();

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        LoadCustomersCommand.Execute(null);
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            LoadCustomersCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            LoadCustomersCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task DeleteCustomer(CustomerListItem customer)
    {
        if (customer == null) return;
        await Task.Run(() => _customerService.Delete(customer.Id));
        LoadCustomersCommand.Execute(null);
    }

    // --- ĐIỀU HƯỚNG ---

    [RelayCommand] private void MapsToAdd() => onAdd();
    [RelayCommand] private void MapsToRules() => onRules();
    [RelayCommand] 
    private void MapsToProfile() 
    {
        if (SelectedCustomer != null) onProfile(SelectedCustomer);
    }
    [RelayCommand] private void EditCustomer(CustomerListItem customer) => onEdit(customer);

    
}

