using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyShop.ViewModels;

/// <summary>
/// Base class cho tất cả ViewModels - implement INotifyPropertyChanged
/// Cung cấp các property và method cơ bản dùng chung
/// </summary>
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // Loading state
    private bool _isLoading = false;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    // Error handling
    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    // Validation errors - key: property name, value: error message
    private Dictionary<string, string> _validationErrors = new();
    public Dictionary<string, string> ValidationErrors
    {
        get => _validationErrors;
        set
        {
            if (_validationErrors != value)
            {
                _validationErrors = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Kích hoạt PropertyChanged event để UI cập nhật
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Set property value và kích hoạt OnPropertyChanged
    /// Sử dụng trong backing fields
    /// </summary>
    protected void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(backingField, value))
        {
            backingField = value;
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Clear error message
    /// </summary>
    public void ClearError()
    {
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Add validation error cho một property
    /// </summary>
    public void AddValidationError(string propertyName, string errorMessage)
    {
        if (!ValidationErrors.ContainsKey(propertyName))
        {
            ValidationErrors[propertyName] = errorMessage;
            OnPropertyChanged(nameof(ValidationErrors));
        }
    }

    /// <summary>
    /// Clear validation errors cho một property
    /// </summary>
    public void ClearValidationError(string propertyName)
    {
        if (ValidationErrors.Remove(propertyName))
        {
            OnPropertyChanged(nameof(ValidationErrors));
        }
    }

    /// <summary>
    /// Clear tất cả validation errors
    /// </summary>
    public void ClearAllValidationErrors()
    {
        ValidationErrors.Clear();
        OnPropertyChanged(nameof(ValidationErrors));
    }
}