using System;

namespace MyShop.ViewModels;

public sealed class CustomerFormViewModel : BaseViewModel
{
    private readonly Action onCancel;
    private bool isEditMode;
    private string formTitle = string.Empty;

    public CustomerFormViewModel(bool isEditMode, CustomerListItem? customer, Action onCancel)
    {
        this.onCancel = onCancel;
        IsEditMode = isEditMode;

        if (isEditMode && customer is not null)
        {
            FormTitle = $"Edit: {customer.Name}";
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
            CustomerData = new CustomerInputModel();
        }

        SubmitCommand = new RelayCommand(Submit);
        CancelCommand = new RelayCommand(() => this.onCancel());
    }

    public bool IsEditMode
    {
        get => isEditMode;
        set
        {
            if (isEditMode != value)
            {
                isEditMode = value;
                OnPropertyChanged();
            }
        }
    }

    public string FormTitle
    {
        get => formTitle;
        set
        {
            if (formTitle != value)
            {
                formTitle = value;
                OnPropertyChanged();
            }
        }
    }

    public string SubmitButtonText => IsEditMode ? "Save Changes" : "Create Customer";

    public CustomerInputModel CustomerData { get; }

    public RelayCommand SubmitCommand { get; }
    public RelayCommand CancelCommand { get; }

    private void Submit()
    {
        onCancel();
    }
}
