using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using System.Threading.Tasks;

namespace MyShop.ViewModels;

public partial class PaymentViewModel : ObservableObject
{
    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _tax;

    [ObservableProperty]
    private decimal _promoDiscount;

    [ObservableProperty]
    private decimal _loyaltyDiscount;

    [ObservableProperty]
    private string _orderIdentifier = "";

    [ObservableProperty]
    private PaymentMethod _method = PaymentMethod.Cash;

    public ObservableCollection<PaymentMethodItem> PaymentMethods { get; } = new();

    [ObservableProperty]
    private string _cashInput = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Change))]
    private decimal _cashAmount;

    public decimal Change => CashAmount - Total;

    public event Action<PaymentMethod>? PaymentConfirmed;

    private readonly IOrderService _orderService;
    private readonly Order _order;

    public PaymentViewModel(Order order, IOrderService orderService, decimal loyaltyDiscount = 0)
    {
        _order = order ?? throw new ArgumentNullException(nameof(order));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        
        // Use order values or fallback to passed discount
        Subtotal = order.SubTotal;
        PromoDiscount = order.DiscountAmount;
        LoyaltyDiscount = Math.Max(order.ManualAdjustment, loyaltyDiscount);
        
        // Standard Tax Rate (align with PosViewModel)
        const decimal TAX_RATE = 0.0875m;
        
        // Calculate correctly: (Subtotal - Discounts) + Tax
        var discountBase = Math.Max(0, Subtotal - PromoDiscount - LoyaltyDiscount);
        Tax = discountBase * TAX_RATE;
        Total = discountBase + Tax;
        
        // Ensure the order object reflects these final calculated values
        _order.TaxAmount = Tax;
        _order.TotalAmount = Total;
        _order.ManualAdjustment = LoyaltyDiscount;
        if (string.IsNullOrEmpty(_order.ManualAdjustmentReason) && LoyaltyDiscount > 0)
        {
            _order.ManualAdjustmentReason = "Loyalty Points Redemption";
        }

        OrderIdentifier = $"ORDER-{order.Id}";
        
        PaymentMethods.Add(new PaymentMethodItem { Method = PaymentMethod.Cash, Label = "Cash", IconKey = "IconBanknote", Glyph = "\uE825", IsSelected = true });
        PaymentMethods.Add(new PaymentMethodItem { Method = PaymentMethod.Card, Label = "Debit/Credit", IconKey = "IconCreditCard", Glyph = "\uE8C7" });
        PaymentMethods.Add(new PaymentMethodItem { Method = PaymentMethod.Digital, Label = "Digital Payment", IconKey = "IconSmartphone", Glyph = "\uE8EA" });
    }

    [RelayCommand]
    private void SelectMethod(string methodStr)
    {
        if (Enum.TryParse<PaymentMethod>(methodStr, out var m))
        {
            Method = m;
            foreach (var item in PaymentMethods)
            {
                item.IsSelected = (item.Method == m);
            }
        }
    }

    [RelayCommand]
    private void HandleKeypad(string key)
    {
        if (key == "C")
        {
            CashInput = "";
        }
        else if (key == "." && CashInput.Contains("."))
        {
            return;
        }
        else
        {
            CashInput += key;
        }

        if (decimal.TryParse(CashInput, out var val))
        {
            CashAmount = val;
        }
        else
        {
            CashAmount = 0;
        }
    }

    [RelayCommand]
    private void AddQuickAmount(string amountStr)
    {
        if (decimal.TryParse(amountStr, out var amount))
        {
            CashAmount += amount;
            CashInput = CashAmount.ToString("F2").Replace(".00", "");
        }
    }

    [RelayCommand]
    private async Task Confirm()
    {
        if (Method == PaymentMethod.Cash && Change < -0.01m) return;
        
        _order.PaymentStatus = "Paid";
        _order.FulfillmentStatus = "None"; // Or Preparing
        _order.OrderDate = DateTime.UtcNow;

        await Task.CompletedTask;
        PaymentConfirmed?.Invoke(Method);
    }

    [RelayCommand]
    public async Task ReturnToDraft()
    {
        _order.PaymentStatus = "Draft";
        await _orderService.Update(_order);
    }
}
