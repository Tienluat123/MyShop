using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using System;

namespace MyShop.ViewModels.StaffManagement;

public partial class EmployeeFormViewModel : ObservableObject
{
    [ObservableProperty] private string _firstName = "", _lastName = "", _username = "", _password = "", _phone = "", _errorMessage = "";
    [ObservableProperty] private decimal _hourlyRate = 25000;
    [ObservableProperty] private bool _isActive = true;
    [ObservableProperty] private int _selectedRoleIndex = 1;

    private User? _originalUser;

    public void Initialize(User? user)
    {
        _originalUser = user;
        if (user == null) return;

        FirstName = user.FirstName ?? "";
        LastName = user.LastName ?? "";
        Username = user.Username ?? "";
        Phone = user.Phone ?? "";
        HourlyRate = user.HourlyRate;
        IsActive = user.IsActive;
        SelectedRoleIndex = user.Role == "1" ? 0 : 1;
    }

    public User GetUpdatedUser()
    {
        var user = _originalUser ?? new User();
        user.FirstName = FirstName.Trim();
        user.LastName = LastName.Trim();
        user.Username = Username.Trim();
        user.Phone = Phone.Trim();
        user.HourlyRate = HourlyRate;
        user.IsActive = IsActive;
        user.Role = SelectedRoleIndex == 0 ? "1" : "2";

        if (!string.IsNullOrWhiteSpace(Password))
            user.PasswordHash = HashPassword(Password);

        return user;
    }

    public bool Validate()
    {
        ErrorMessage = (FirstName, LastName, Username, Password, HourlyRate) switch
        {
            (var f, var l, _, _, _) when string.IsNullOrWhiteSpace(f) || string.IsNullOrWhiteSpace(l) => "First/Last name required",
            (var u, _, _, _, _) when string.IsNullOrWhiteSpace(u) => "Username required",
            (_, _, _, var p, _) when _originalUser == null && string.IsNullOrWhiteSpace(p) => "Password required for new staff",
            (_, _, _, _, var r) when r <= 0 => "Hourly rate must be > 0",
            _ => ""
        };

        return string.IsNullOrEmpty(ErrorMessage);
    }

    [RelayCommand]
    private void Save() => Validate();

    [RelayCommand]
    private void Cancel() => Reset();

    private void Reset()
    {
        _originalUser = null;
        FirstName = LastName = Username = Password = Phone = ErrorMessage = "";
        HourlyRate = 25000;
        IsActive = true;
        SelectedRoleIndex = 1;
    }

    private string HashPassword(string password)
    {
        // Sử dụng BCrypt để băm mật khẩu, đồng bộ với logic kiểm tra trong AuthService
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}