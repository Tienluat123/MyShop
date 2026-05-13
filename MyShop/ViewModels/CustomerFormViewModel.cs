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
    private readonly INotificationService? _notificationService;
    private readonly Action _onSuccess;
    private readonly int _editingCustomerId = 0;
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

    [ObservableProperty] private bool isLoading = false;

    public CustomerFormViewModel(ICustomerService customerService, bool isEditMode, CustomerListItem? customer, Action onSuccess, INotificationService? notificationService = null)
    {
        _customerService = customerService;
        _notificationService = notificationService;
        _onSuccess = onSuccess;
        IsEditMode = isEditMode;

        if (isEditMode && customer is not null)
        {
            _editingCustomerId = customer.Id;
            FormTitle = $"Edit: {customer.Name}";
            SubmitButtonText = "Save Changes";

            // Nạp dữ liệu ban đầu từ customer item
            CustomerData = new CustomerInputModel
            {
                FullName = customer.Name,
                Phone = customer.Phone,
                Email = customer.Email ?? "",
                Gender = customer.Gender ?? "Male",
                Address = "",
                Notes = "",
                Birthday = customer.Birthday.HasValue ? new DateTimeOffset(customer.Birthday.Value) : null
            };

            // Async load chi tiết từ database (không chặn constructor)
            _ = LoadCustomerDetailsAsync(customer.Id);
        }
        else
        {
            FormTitle = "Create New Customer";
            SubmitButtonText = "Create Customer";
        }
    }

    // Tải chi tiết customer từ DB (async, không block constructor)
    private async Task LoadCustomerDetailsAsync(int customerId)
    {
        try
        {
            IsLoading = true;
            var fullInfo = await _customerService.GetById(customerId);

            if (fullInfo != null)
            {
                CustomerData = new CustomerInputModel
                {
                    FullName = fullInfo.Name,
                    Phone = fullInfo.Phone,
                    Email = fullInfo.Email ?? "",
                    Gender = fullInfo.Gender ?? "Male",
                    Address = fullInfo.Address ?? "",
                    Notes = fullInfo.Notes ?? "",
                    Birthday = fullInfo.Birthday.HasValue ? new DateTimeOffset(fullInfo.Birthday.Value) : null
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] Load customer details failed: {ex.Message}");
            _notificationService?.ShowError("Unable to load customer details");
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private async Task Submit()
    {
        Console.WriteLine("[DEBUG] Submit button clicked...");

        if (!ValidateForm())
        {
            Console.WriteLine($"[DEBUG] Validate FAILED: NameErr={NameError}, PhoneErr={PhoneError}");
            return;
        }

        Console.WriteLine("[DEBUG] Validate SUCCESS. Preparing to save...");

        try
        {
            IsLoading = true;

            if (IsEditMode)
            {
                if (_editingCustomerId <= 0)
                {
                    _notificationService?.ShowError("Invalid customer ID");
                    return;
                }

                var existing = await _customerService.GetById(_editingCustomerId);
                if (existing == null)
                {
                    _notificationService?.ShowError("Customer not found for update");
                    return;
                }

                var customer = new Customer
                {
                    Id = existing.Id,
                    Name = CustomerData.FullName,
                    Phone = CustomerData.Phone,
                    Email = CustomerData.Email,
                    Gender = CustomerData.Gender,
                    Address = CustomerData.Address,
                    Notes = CustomerData.Notes,
                    Birthday = CustomerData.Birthday?.DateTime,
                    CreatedAt = existing.CreatedAt,
                    Status = existing.Status,
                    TotalAccumulatedPoints = existing.TotalAccumulatedPoints,
                    CurrentAvailablePoints = existing.CurrentAvailablePoints,
                    TotalVisits = existing.TotalVisits,
                    TotalSpent = existing.TotalSpent,
                    LastVisitDate = existing.LastVisitDate,
                    Tier = existing.Tier
                };

                Console.WriteLine($"[DEBUG] Calling UPDATE for ID: {customer.Id}");
                await _customerService.Update(customer);
                _notificationService?.ShowSuccess("Customer updated successfully!");
            }
            else
            {
                var customer = new Customer
                {
                    Id = 0,
                    Name = CustomerData.FullName,
                    Phone = CustomerData.Phone,
                    Email = CustomerData.Email,
                    Gender = CustomerData.Gender,
                    Address = CustomerData.Address,
                    Notes = CustomerData.Notes,
                    Birthday = CustomerData.Birthday?.DateTime,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active",
                    TotalAccumulatedPoints = 0,
                    CurrentAvailablePoints = 0,
                    TotalVisits = 0,
                    TotalSpent = 0
                };

                Console.WriteLine("[DEBUG] Calling ADD for new customer");
                await _customerService.Add(customer);
                _notificationService?.ShowSuccess("Customer added successfully!");
            }

            Console.WriteLine("[DEBUG] Database save SUCCESS!");
            _onSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            var errorMsg = BuildDetailedErrorMessage(ex);
            Console.WriteLine($"[ERROR] Submit failed:\n{errorMsg}");
            _notificationService?.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Build full exception chain message for debugging
    /// </summary>
    private string BuildDetailedErrorMessage(Exception ex)
    {
        var messages = new List<string>();
        var current = ex;
        int level = 0;

        while (current != null)
        {
            string indent = new string(' ', level * 2);
            messages.Add($"{indent}└─ {current.GetType().Name}: {current.Message}");
            current = current.InnerException;
            level++;
        }

        return string.Join("\n", messages);
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
