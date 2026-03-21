using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;

namespace MyShop.ViewModels;

public partial class PaymentViewModel : ObservableObject
{
    [ObservableProperty]
    private double _total;

    [ObservableProperty]
    private string _orderIdentifier = "";

    [ObservableProperty]
    private PaymentMethod _method = PaymentMethod.Cash;

    public ObservableCollection<PaymentMethodItem> PaymentMethods { get; } = new();

    [ObservableProperty]
    private string _cashInput = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Change))]
    private double _cashAmount;

    public double Change => CashAmount - Total;

    public event Action<PaymentMethod>? PaymentConfirmed;

    public PaymentViewModel(double total, string orderId)
    {
        Total = total;
        OrderIdentifier = orderId;
        
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

        if (double.TryParse(CashInput, out var val))
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
        if (double.TryParse(amountStr, out var amount))
        {
            CashAmount += amount;
            CashInput = CashAmount.ToString("F2").Replace(".00", "");
        }
    }

    [RelayCommand]
    private void Confirm()
    {
        if (Method == PaymentMethod.Cash && Change < -0.01) return;
        PaymentConfirmed?.Invoke(Method);
    }
}
