using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;

namespace MyShop.ViewModels;

public partial class ProductCustomizationViewModel : ObservableObject
{
    [ObservableProperty]
    private TempProduct _product;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private int _quantity = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnitTotalPrice))]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    private string _size = "M";

    [ObservableProperty]
    private string _sugar = "100%";

    [ObservableProperty]
    private string _ice = "Regular";

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

    public double UnitTotalPrice => CalculateBasePrice();
    public double TotalPrice => UnitTotalPrice * Quantity;

    public event Action<DrinkOptions, int>? DrinkConfirmed;

    public ProductCustomizationViewModel(TempProduct product, OrderItem? existingItem = null)
    {
        Product = product;
        if (existingItem != null)
        {
            Quantity = existingItem.Quantity;
            //if (existingItem.DrinkOptions != null)
            //{
            //    Size = existingItem.DrinkOptions.Size;
            //    Sugar = existingItem.DrinkOptions.Sugar;
            //    Ice = existingItem.DrinkOptions.Ice;
            //    Notes = existingItem.DrinkOptions.Notes;
            //    foreach (var addOn in existingItem.DrinkOptions.AddOns) AddOns.Add(addOn);
            //}
        }

        AddOns.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(UnitTotalPrice));
            OnPropertyChanged(nameof(TotalPrice));
        };
    }

    private double CalculateBasePrice()
    {
        double price = Product.Price;
        if (Product.Category == "Coffee" || Product.Category == "Tea")
        {
            if (Size == "M") price += 0.5;
            else if (Size == "L") price += 1.0;
            
            // Add-ons are $0.75 each in the web version
            price += AddOns.Count * 0.75;
        }
        return price;
    }

    [RelayCommand]
    private void Confirm()
    {
        var drinkOpts = new DrinkOptions
        {
            Size = Size,
            Sugar = Sugar,
            Ice = Ice,
            Notes = Notes
        };
        foreach (var addOn in AddOns) drinkOpts.AddOns.Add(addOn);

        DrinkConfirmed?.Invoke(drinkOpts, Quantity);
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
            OnPropertyChanged(nameof(TotalPrice));
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
}
