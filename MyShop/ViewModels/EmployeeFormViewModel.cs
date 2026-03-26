using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MyShop.ViewModels;

public partial class EmployeeData : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private int position; // 0 = Admin, 1 = Bán hàng

    [ObservableProperty]
    private int status; // 0 = Đang làm việc, 1 = Nghỉ phép

    [ObservableProperty]
    private string notes = string.Empty;
}

public partial class EmployeeFormViewModel : ObservableObject
{
    private readonly Action onCancel;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private string formTitle = "Thêm nhân viên mới";

    [ObservableProperty]
    private string submitButtonText = "Lưu thông tin";

    [ObservableProperty]
    private EmployeeData employeeData = new();

    public EmployeeFormViewModel(bool isEditMode, EmployeeData? employee, Action onCancel)
    {
        this.onCancel = onCancel;
        IsEditMode = isEditMode;

        if (isEditMode && employee is not null)
        {
            FormTitle = "Chỉnh sửa thông tin nhân viên";
            SubmitButtonText = "Cập nhật thay đổi";
            EmployeeData = employee;
        }
        else
        {
            FormTitle = "Thêm nhân viên mới";
            SubmitButtonText = "Lưu thông tin";
            EmployeeData = new EmployeeData();
        }
    }

    [RelayCommand]
    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(EmployeeData.Name) || string.IsNullOrWhiteSpace(EmployeeData.Phone))
            return;

        onCancel();
    }

    [RelayCommand]
    private void Cancel()
    {
        onCancel();
    }
}
