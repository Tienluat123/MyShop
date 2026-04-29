using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public enum PaymentMethod
{
    Cash,
    Card,
    Digital
}

public partial class PaymentMethodItem : ObservableObject
{
    public PaymentMethod Method { get; set; }
    public string Label { get; set; } = "";
    public string IconKey { get; set; } = "";
    public string Glyph { get; set; } = "";

    [ObservableProperty]
    private bool _isSelected;
}

public enum PaymentStatus
{
    Draft,
    PendingPayment,
    Paid,
    Cancelled,
    Refunded
}

public enum FulfillmentStatus
{
    None,
    Preparing,
    Ready,
    Completed
}

public partial class DrinkOptions : ObservableObject
{
    [ObservableProperty]
    private string _size = "M";

    [ObservableProperty]
    private string _sugar = "100%";

    [ObservableProperty]
    private string _ice = "Regular";

    [ObservableProperty]
    private ObservableCollection<string> _addOns = new();

    [ObservableProperty]
    private string _notes = "";
}

public partial class OrderItem : ObservableObject
{
    public Product Product { get; set; } = null!;

    public decimal BasePrice { get; set; }

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private DrinkOptions? _drinkOptions;

    public decimal TotalPrice => BasePrice * Quantity;

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(TotalPrice));
    }
}

public partial class PosOrder : ObservableObject
{
    [ObservableProperty]
    private string _orderId = "";

    public ObservableCollection<OrderItem> Items { get; } = new();

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _tax;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private string _type = "Dine-in";

    [ObservableProperty]
    private string? _table;

    [ObservableProperty]
    private PaymentMethod _method;

    [ObservableProperty]
    private PaymentStatus _paymentStatus = PaymentStatus.Draft;

    [ObservableProperty]
    private FulfillmentStatus _fulfillmentStatus = FulfillmentStatus.None;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;

    [ObservableProperty]
    private string _notes = "";

    [ObservableProperty]
    private decimal _discount;

    [ObservableProperty]
    private decimal _manualAdjustment;

    [ObservableProperty]
    private string _manualAdjustmentReason = "";

    [ObservableProperty]
    private string? _customerId;

    [ObservableProperty]
    private string? _customerName;

    public bool IsFulfillmentActionEnabled => 
        !(FulfillmentStatus == FulfillmentStatus.Ready && PaymentStatus != PaymentStatus.Paid);

    partial void OnPaymentStatusChanged(PaymentStatus value) => OnPropertyChanged(nameof(IsFulfillmentActionEnabled));
    partial void OnFulfillmentStatusChanged(FulfillmentStatus value) => OnPropertyChanged(nameof(IsFulfillmentActionEnabled));
}


public class UpdateQtyParams
{
    public OrderItem? Item { get; set; }
    public int Delta { get; set; }
}

public class AddCustomizedParams
{
    public Product Product { get; set; } = null!;
    public DrinkOptions Options { get; set; } = null!;
    public int Quantity { get; set; }
}

public partial class CategoryItem : ObservableObject
{
    public string Name { get; set; } = "";
    public string IconKey { get; set; } = "";

    [ObservableProperty]
    private bool _isActive;
}
