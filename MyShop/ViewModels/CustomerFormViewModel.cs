using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using System.Text.RegularExpressions;
namespace MyShop.ViewModels;

public partial class CustomerFormViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly Action _onSuccess;
    private readonly int? _editingCustomerId;
    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string formTitle = string.Empty;

    [ObservableProperty]
    private string submitButtonText = "Create Customer";

    [ObservableProperty]
    private CustomerInputModel customerData = new();

    [ObservableProperty] private string? nameError;
    [ObservableProperty] private string? phoneError;
    [ObservableProperty] private string? emailError;
    public CustomerFormViewModel(ICustomerService customerService, bool isEditMode, CustomerListItem? customer, Action onSuccess)
    {
        _customerService = customerService;
        _onSuccess = onSuccess;
        IsEditMode = isEditMode;

        if (isEditMode && customer is not null)
        {
            _editingCustomerId = customer.Id;
            FormTitle = $"Edit: {customer.Name}";
            SubmitButtonText = "Save Changes";

            // PHẢI NẠP ĐỦ THÔNG TIN Ở ĐÂY ĐỂ UI CÓ DỮ LIỆU CŨ
            // Lấy thông tin chi tiết từ Service vì CustomerListItem thường thiếu Address, Email
            var fullInfo = _customerService.GetById(customer.Id).GetAwaiter().GetResult();

            CustomerData = new CustomerInputModel
            {
                FullName = fullInfo?.Name ?? customer.Name,
                Phone = fullInfo?.Phone ?? customer.Phone,
                Email = fullInfo?.Email ?? "",
                Address = fullInfo?.Address ?? "",
                Notes = fullInfo?.Notes ?? "",
                Birthday = fullInfo?.Birthday.HasValue == true ? new DateTimeOffset(fullInfo.Birthday.Value) : null
            };
        }
        else
        {
            FormTitle = "Create New Customer";
            SubmitButtonText = "Create Customer";
            //CustomerData = new CustomerInputModel();
        }
    }
    [RelayCommand]
    private async Task Submit()
    {
        Console.WriteLine("[DEBUG] Đang nhấn nút Submit...");

        if (!ValidateForm())
        {
            Console.WriteLine($"[DEBUG] Validate THẤT BẠI: NameErr={NameError}, PhoneErr={PhoneError}");
            return;
        }

        Console.WriteLine("[DEBUG] Validate THÀNH CÔNG. Đang chuẩn bị lưu...");

        var customer = new Customer
        {
            Id = _editingCustomerId ?? 0,
            Name = CustomerData.FullName,
            Phone = CustomerData.Phone,
            Email = CustomerData.Email,
            Address = CustomerData.Address,
            Notes = CustomerData.Notes,
            Birthday = CustomerData.Birthday?.DateTime,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            if (IsEditMode)
            {
                Console.WriteLine($"[DEBUG] Đang gọi UPDATE cho ID: {customer.Id}");
                await Task.Run(() => _customerService.Update(customer));
            }
            else
            {
                Console.WriteLine("[DEBUG] Đang gọi ADD khách hàng mới");
                await Task.Run(() => _customerService.Add(customer));
            }

            Console.WriteLine("[DEBUG] Lưu Database THÀNH CÔNG!");
            _onSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            // In lỗi chi tiết nhất có thể
            var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            Console.WriteLine($"[DEBUG] LỖI DATABASE THỰC SỰ: {errorMsg}");
        }
    }
    [RelayCommand]
    private void Cancel() => _onSuccess?.Invoke();
    private bool ValidateForm()
    {
        ValidateName();
        ValidatePhone();
        ValidateEmail();
        return string.IsNullOrEmpty(NameError) && string.IsNullOrEmpty(PhoneError) && string.IsNullOrEmpty(EmailError);
    }

    [RelayCommand]
    public void ValidateName()
    // Sửa nameError -> NameError
    => NameError = string.IsNullOrWhiteSpace(CustomerData.FullName) ? "Name is required" : null;

    [RelayCommand]
    public void ValidatePhone()
    {
        if (string.IsNullOrWhiteSpace(CustomerData.Phone))
            PhoneError = "Phone is required"; // Sửa phoneError -> PhoneError
        else if (!Regex.IsMatch(CustomerData.Phone, @"^[0-9]{10,11}$"))
            PhoneError = "10-11 digits required";
        else
            PhoneError = null;
    }

    [RelayCommand]
    public void ValidateEmail()
    {
        if (string.IsNullOrEmpty(CustomerData.Email)) { EmailError = null; return; }
        if (!Regex.IsMatch(CustomerData.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            EmailError = "Invalid email format"; // Sửa emailError -> EmailError
        else
            EmailError = null;
    }
}
