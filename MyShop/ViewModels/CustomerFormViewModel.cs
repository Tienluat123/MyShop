using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public partial class CustomerFormViewModel : ObservableObject
{
    private readonly Action onCancel;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string formTitle = string.Empty;

    [ObservableProperty]
    private string submitButtonText = "Create Customer";

    [ObservableProperty]
    private CustomerInputModel customerData = new();

    public CustomerFormViewModel(bool isEditMode, CustomerListItem? customer, Action onCancel)
    {
        this.onCancel = onCancel;
        IsEditMode = isEditMode;

        if (isEditMode && customer is not null)
        {
            FormTitle = $"Edit: {customer.Name}";
            SubmitButtonText = "Save Changes";
            CustomerData = new CustomerInputModel
            {
                FullName = customer.Name,
                Phone = customer.Phone,
                Email = "vana@example.com",
                Birthday = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero),
                Address = "123 Le Loi, District 1, HCMC",
                Notes = ""
            };
        }
        else
        {
            FormTitle = "Create New Customer";
            SubmitButtonText = "Create Customer";
            CustomerData = new CustomerInputModel();
        }
    }

    [RelayCommand]
    private void Submit()
    {
        onCancel();
    }

    [RelayCommand]
    private void Cancel()
    {
        onCancel();
    }
}
