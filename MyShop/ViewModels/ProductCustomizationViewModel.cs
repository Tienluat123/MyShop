using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;

namespace MyShop.ViewModels;

public partial class ProductCustomizationViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentImageUrl))]
    [NotifyPropertyChangedFor(nameof(HasCurrentImage))]
    [NotifyPropertyChangedFor(nameof(HasMultipleImages))]
    [NotifyPropertyChangedFor(nameof(ImagePositionText))]
    private Product _product;

    private OrderItem? _editingItem;
    private IReadOnlyList<string> ProductImageUrls => BuildProductImageUrls();
    public bool IsEditing => _editingItem != null;
    public string Title => IsEditing ? $"Edit {Product.Name}" : $"Customize {Product.Name}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentImageUrl))]
    [NotifyPropertyChangedFor(nameof(ImagePositionText))]
    private int _currentImageIndex;

    public string? CurrentImageUrl => ProductImageUrls.ElementAtOrDefault(CurrentImageIndex);
    public bool HasCurrentImage => !string.IsNullOrWhiteSpace(CurrentImageUrl);
    public bool HasMultipleImages => ProductImageUrls.Count > 1;
    public string ImagePositionText => HasMultipleImages
        ? $"{CurrentImageIndex + 1} / {ProductImageUrls.Count}"
        : string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private int _quantity = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnitTotalPrice))]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private string _size = "S";

    [ObservableProperty]
    private string _sugar = "100%";

    [ObservableProperty]
    private string _ice = "Regular";

    // UI-friendly addon options with selected state
    public ObservableCollection<AddOnOption> AddOnOptions { get; } = new();
    // Backing collection used when confirming the item
    public ObservableCollection<string> AddOns { get; } = new();

    public string[] AvailableAddOns { get; } = new[]
    {
        "Extra Shot", "Caramel Syrup", "Vanilla Syrup", "Whipped Cream", "Oat Milk"
    };

    public string[] IceOptions { get; } = new[]
    {
        "No Ice", "Less Ice", "Regular", "Extra Ice"
    };

    [ObservableProperty]
    private string _notes = "";

    // Simple addon option with selection state for UI binding
    public class AddOnOption : System.ComponentModel.INotifyPropertyChanged
    {
        public string Name { get; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    IsSelectedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? IsSelectedChanged;
        public AddOnOption(string name, bool isSelected = false)
        {
            Name = name;
            _isSelected = isSelected;
        }
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public double UnitTotalPrice => CalculateBasePrice();
    public double TotalPrice => UnitTotalPrice * Quantity;

    //public event Action<DrinkOptions, int>? DrinkConfirmed;

    public event Action<OrderItem>? ItemConfirmed;

    public ProductCustomizationViewModel(Product product, OrderItem? existingItem = null)
    {
        Product = product;
        _editingItem = existingItem;

        // Attach collection-changed handler first to ensure UI updates when addons are pre-populated
        // Wire up addon option changes to reflect in AddOns list and prices
        AddOnOptions.CollectionChanged += (s, e) =>
        {
            // no-op, options replaced by explicit population
        };

        if (existingItem != null)
        {
            Quantity = existingItem.Quantity;

            if (existingItem.DrinkOptions != null && HasCustomization)
            {
                Size = existingItem.DrinkOptions.Size;
                Sugar = existingItem.DrinkOptions.Sugar;
                Ice = existingItem.DrinkOptions.Ice;
                Notes = existingItem.DrinkOptions.Notes;

                foreach (var addOn in existingItem.DrinkOptions.AddOns)
                    AddOns.Add(addOn);
            }
        }
        
        // Initialize UI addon options from available addons and sync initial selection
        if (HasCustomization)
        {
            foreach (var addon in AvailableAddOns)
            {
                bool isSelected = existingItem?.DrinkOptions?.AddOns.Contains(addon) == true;
                AddOnOptions.Add(new AddOnOption(addon, isSelected));
            }
        }
        foreach (var opt in AddOnOptions)
        {
            opt.IsSelectedChanged += (s, e) =>
            {
                if (opt.IsSelected)
                {
                    if (!AddOns.Contains(opt.Name)) AddOns.Add(opt.Name);
                }
                else
                {
                    AddOns.Remove(opt.Name);
                }
                OnPropertyChanged(nameof(UnitTotalPrice));
                OnPropertyChanged(nameof(TotalPrice));
            };
        }
        
        // Ensure UI reflects initial addon selections
        foreach (var opt in AddOnOptions)
        {
            if (opt.IsSelected && !AddOns.Contains(opt.Name))
            {
                AddOns.Add(opt.Name);
            }
            opt.IsSelectedChanged += (s, e) =>
            {
                if (opt.IsSelected)
                {
                    if (!AddOns.Contains(opt.Name)) AddOns.Add(opt.Name);
                }
                else
                {
                    AddOns.Remove(opt.Name);
                }
                OnPropertyChanged(nameof(UnitTotalPrice));
                OnPropertyChanged(nameof(TotalPrice));
            };
        }
    }

    private double CalculateBasePrice()
    {
        double price = (double)Product.Price;
        if (Product.Category?.HasCustomization == true)
        {
            if (Size == "M") price += 15000;
            else if (Size == "L") price += 25000;

            price += AddOns.Count * 10000;
        }
        return price;
    }

    private IReadOnlyList<string> BuildProductImageUrls()
    {
        if (Product is null)
        {
            return Array.Empty<string>();
        }

        var imageUrls = Product.ProductImages
            .OrderBy(pi => pi.SortOrder)
            .Select(pi => pi.ImageUrl)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .ToList();

        if (!string.IsNullOrWhiteSpace(Product.ImagePath))
        {
            imageUrls.Add(Product.ImagePath);
        }

        return imageUrls
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    [RelayCommand]
    private void ShowPreviousImage()
    {
        var imageCount = ProductImageUrls.Count;
        if (imageCount <= 1)
        {
            return;
        }

        CurrentImageIndex = (CurrentImageIndex - 1 + imageCount) % imageCount;
    }

    [RelayCommand]
    private void ShowNextImage()
    {
        var imageCount = ProductImageUrls.Count;
        if (imageCount <= 1)
        {
            return;
        }

        CurrentImageIndex = (CurrentImageIndex + 1) % imageCount;
    }

    [RelayCommand]
    private void Confirm()
    {
        bool customizable = HasCustomization;
        DrinkOptions? drinkOpts = null;
        if (customizable)
        {
            drinkOpts = new DrinkOptions
            {
                Size = Size,
                Sugar = Sugar,
                Ice = Ice,
                Notes = Notes
            };
            foreach (var addOn in AddOns)
                drinkOpts.AddOns.Add(addOn);
        }

        var item = _editingItem ?? new OrderItem();

        item.Product = Product;
        item.Quantity = Quantity;
        item.DrinkOptions = drinkOpts;
        item.BasePrice = (decimal)UnitTotalPrice;

        // Only persist items with customization
        ItemConfirmed?.Invoke(item);
    }

    [RelayCommand]
    private void ToggleAddOn(string addOn)
    {
        if (AddOns.Contains(addOn)) AddOns.Remove(addOn);
        else AddOns.Add(addOn);
    }

    [RelayCommand]
    private void UpdateQuantity(object parameter)
    {
        if (parameter != null && int.TryParse(parameter.ToString(), out int delta))
        {
            Quantity = Math.Max(1, Quantity + delta);
        }
    }

    [RelayCommand]
    private void SetSize(string size)
    {
        Size = size;
    }

    [RelayCommand]
    private void SetIce(string ice)
    {
        Ice = ice;
    }
    
    public bool HasCustomization => Product?.Category?.HasCustomization == true;
}
