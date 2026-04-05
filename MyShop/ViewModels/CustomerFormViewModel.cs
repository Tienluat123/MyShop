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
            CustomerData = new CustomerInputModel
            {
                FullName = customer.Name,
                Phone = customer.Phone,
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
    private async Task SaveCustomer()
    {
        if (!ValidateForm()) return;

        // Map từ Input Model sang Database Model
        var customer = new Customer {
            Id = _editingCustomerId ?? 0,
            Name = CustomerData.FullName,
            Phone = CustomerData.Phone,
            Email = CustomerData.Email,
            Address = CustomerData.Address,
            Notes = CustomerData.Notes,
            Birthday = CustomerData.Birthday?.DateTime,
            CreatedAt = DateTime.UtcNow
        };

        try {
            if (IsEditMode)
                await Task.Run(() => _customerService.Update(customer));
            else
                await Task.Run(() => _customerService.Add(customer));

            _onSuccess?.Invoke(); // Đóng dialog và refresh danh sách
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Lỗi lưu DB: {ex.Message}");
        }
    }

    private bool ValidateForm()
    {
        ValidateName();
        ValidatePhone();
        ValidateEmail();
        return string.IsNullOrEmpty(nameError) && string.IsNullOrEmpty(PhoneError) && string.IsNullOrEmpty(EmailError);
    }

    [RelayCommand]
    private void ValidateName() => nameError = string.IsNullOrWhiteSpace(CustomerData.FullName) ? "Name is required" : null;

    [RelayCommand]
    private void ValidatePhone()
    {
        if (string.IsNullOrWhiteSpace(CustomerData.Phone)) phoneError = "Phone is required";
        else if (!Regex.IsMatch(CustomerData.Phone, @"^[0-9]{10,11}$")) phoneError = "10-11 digits required";
        else phoneError = null;
    }

    [RelayCommand]
    private void ValidateEmail()
    {
        if (string.IsNullOrEmpty(CustomerData.Email)) { emailError = null; return; }
        if (!Regex.IsMatch(CustomerData.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) emailError = "Invalid email format";
        else emailError = null;
    }
    }
