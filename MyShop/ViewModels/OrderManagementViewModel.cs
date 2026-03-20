using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;

namespace MyShop.ViewModels;

public partial class OrderManagementViewModel : ObservableObject
{
    private readonly ObservableCollection<PosOrder> _allOrders;

    [ObservableProperty]
    private string _filter = "Active"; // "Active" or "History"

    [ObservableProperty]
    private int _activeCount;

    [ObservableProperty]
    private int _historyCount;

    public ObservableCollection<PosOrder> DisplayOrders { get; } = new();

    public OrderManagementViewModel(ObservableCollection<PosOrder> orders)
    {
        _allOrders = orders;
        if (!_allOrders.Any())
        {
            LoadSampleData();
        }
        UpdateDisplayOrders();
        _allOrders.CollectionChanged += (s, e) => UpdateDisplayOrders();
    }

    private void LoadSampleData()
    {
        var coffee = new TempProduct { Name = "Caramel Macchiato", Price = 5.50, Category = "Coffee" };
        var muffin = new TempProduct { Name = "Blueberry Muffin", Price = 3.25, Category = "Pastries" };
        var tea = new TempProduct { Name = "Green Tea", Price = 4.00, Category = "Tea" };

        _allOrders.Add(new PosOrder
        {
            OrderId = "ORD-1234",
            Status = OrderStatus.Pending,
            Type = "Dine-in",
            Table = "5",
            CreatedAt = DateTime.Now.AddMinutes(-5),
            Items = { new OrderItem { Product = coffee, Quantity = 2, BasePrice = 5.50 } }
        });

        _allOrders.Add(new PosOrder
        {
            OrderId = "ORD-5678",
            Status = OrderStatus.Preparing,
            Type = "Takeaway",
            CreatedAt = DateTime.Now.AddMinutes(-12),
            Items = { 
                new OrderItem { Product = tea, Quantity = 1, BasePrice = 4.00 },
                new OrderItem { Product = muffin, Quantity = 1, BasePrice = 3.25 }
            }
        });

        _allOrders.Add(new PosOrder
        {
            OrderId = "ORD-9012",
            Status = OrderStatus.Completed,
            Type = "Dine-in",
            Table = "12",
            CreatedAt = DateTime.Now.AddHours(-1),
            Items = { new OrderItem { Product = coffee, Quantity = 1, BasePrice = 5.50 } }
        });
    }

    [RelayCommand]
    private void SetFilter(string filterValue)
    {
        Filter = filterValue;
    }

    [RelayCommand]
    private void UpdateStatus(PosOrder order)
    {
        if (order.Status == OrderStatus.Pending) order.Status = OrderStatus.Preparing;
        else if (order.Status == OrderStatus.Preparing) order.Status = OrderStatus.Ready;
        else if (order.Status == OrderStatus.Ready) order.Status = OrderStatus.Completed;
        
        UpdateDisplayOrders();
    }

    [RelayCommand]
    private void VoidOrder(PosOrder order)
    {
        order.Status = OrderStatus.Voided;
        UpdateDisplayOrders();
    }

    [RelayCommand]
    private void RefundOrder(PosOrder order)
    {
        order.Status = OrderStatus.Refunded;
        UpdateDisplayOrders();
    }

    [RelayCommand]
    private void ClearHistory()
    {
        var activeStatuses = new[] { OrderStatus.Pending, OrderStatus.Preparing, OrderStatus.Ready };
        var historyOrders = _allOrders.Where(o => !activeStatuses.Contains(o.Status)).ToList();
        foreach (var order in historyOrders)
        {
            _allOrders.Remove(order);
        }
    }

    partial void OnFilterChanged(string value) => UpdateDisplayOrders();

    private void UpdateDisplayOrders()
    {
        var activeStatuses = new[] { OrderStatus.Pending, OrderStatus.Preparing, OrderStatus.Ready };
        
        ActiveCount = _allOrders.Count(o => activeStatuses.Contains(o.Status));
        HistoryCount = _allOrders.Count(o => !activeStatuses.Contains(o.Status));

        DisplayOrders.Clear();
        var filtered = Filter == "Active" 
            ? _allOrders.Where(o => activeStatuses.Contains(o.Status))
            : _allOrders.Where(o => !activeStatuses.Contains(o.Status));

        foreach (var order in filtered)
        {
            DisplayOrders.Add(order);
        }
    }
}
