using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class ConfigViewModel : ObservableObject
{
    // ===== Shop Information =====
    [ObservableProperty] private string _shopName = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _shopCode = string.Empty;
    [ObservableProperty] private string _contactEmail = string.Empty;

    // ===== Display Settings =====
    [ObservableProperty] private int _itemsPerPage = 20;

    // Derived index for ComboBox binding (5→0, 10→1, 20→2, 50→3)
    private static readonly int[] PageSizeOptions = { 5, 10, 20, 50 };

    [ObservableProperty]
    private int _itemsPerPageIndex = 2; // default: 20 items

    // ===== Functionality Settings =====
    [ObservableProperty] private bool _autoSaveLastScreen = true;

    // ===== Validation =====
    [ObservableProperty] private string _validationMessage = string.Empty;
    [ObservableProperty] private bool _hasValidationError = false;

    private readonly IAppConfigService _configService;

    // Config key constants — must match the keys in SeedData/08-appconfig.sql
    private const string KeyShopName = "ShopName";
    private const string KeyPhone = "Phone";
    private const string KeyAddress = "Address";
    private const string KeyShopCode = "ShopCode";
    private const string KeyContactEmail = "ContactEmail";
    private const string KeyItemsPerPage = "ItemsPerPage";
    private const string KeyAutoSaveLastScreen = "AutoSaveLastScreen";

    public ConfigViewModel(IAppConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _ = LoadAllSettings();
    }

    [RelayCommand]
    private async Task LoadAllSettings()
    {
        try
        {
            ShopName = await _configService.GetAsync(KeyShopName, "MyShop");
            Phone = await _configService.GetAsync(KeyPhone, "");
            Address = await _configService.GetAsync(KeyAddress, "");
            ShopCode = await _configService.GetAsync(KeyShopCode, "");
            ContactEmail = await _configService.GetAsync(KeyContactEmail, "");

            var itemsPerPageStr = await _configService.GetAsync(KeyItemsPerPage, "20");
            ItemsPerPage = int.TryParse(itemsPerPageStr, out var ipp) ? ipp : 20;

            var autoSaveStr = await _configService.GetAsync(KeyAutoSaveLastScreen, "True");
            AutoSaveLastScreen = autoSaveStr.Equals("True", StringComparison.OrdinalIgnoreCase);

            // Sync ComboBox index with loaded itemsPerPage
            var idx = Array.IndexOf(PageSizeOptions, ItemsPerPage);
            ItemsPerPageIndex = idx >= 0 ? idx : 2;

            ClearValidation();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            SetValidationError("Failed to load settings. Please try again.");
        }
    }

    // Keep ItemsPerPage in sync when ComboBox index changes
    partial void OnItemsPerPageIndexChanged(int value)
    {
        if (value >= 0 && value < PageSizeOptions.Length)
            _itemsPerPage = PageSizeOptions[value];
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        // Validate
        if (!ValidateSettings()) return;

        try
        {
            await _configService.SetAsync(KeyShopName, ShopName);
            await _configService.SetAsync(KeyPhone, Phone);
            await _configService.SetAsync(KeyAddress, Address);
            await _configService.SetAsync(KeyShopCode, ShopCode);
            await _configService.SetAsync(KeyContactEmail, ContactEmail);
            await _configService.SetAsync(KeyItemsPerPage, ItemsPerPage.ToString());
            await _configService.SetAsync(KeyAutoSaveLastScreen, AutoSaveLastScreen.ToString());

            ClearValidation();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            SetValidationError("Failed to save settings. Please try again.");
        }
    }

    private bool ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(ShopName))
        {
            SetValidationError("Shop Name is required.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Phone) && !IsValidPhone(Phone))
        {
            SetValidationError("Phone number format is invalid.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(ContactEmail) && !IsValidEmail(ContactEmail))
        {
            SetValidationError("Email format is invalid.");
            return false;
        }

        if (ItemsPerPage < 1)
        {
            SetValidationError("Items Per Page must be at least 1.");
            return false;
        }

        ClearValidation();
        return true;
    }

    private static bool IsValidPhone(string phone)
    {
        // Accepts digits, spaces, dashes, parentheses, and leading + sign
        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[\d\s\-\+\(\)]{7,15}$");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void SetValidationError(string message)
    {
        ValidationMessage = message;
        HasValidationError = true;
    }

    private void ClearValidation()
    {
        ValidationMessage = string.Empty;
        HasValidationError = false;
    }
}
