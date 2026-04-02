using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using MyShop.Data;

namespace MyShop.ViewModels;

public partial class OrderManagementViewModel : ObservableObject
{
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private string _filter = "Active"; // "Active" or "History"

    [ObservableProperty]
    private int _activeCount;

    [ObservableProperty]
    private int _historyCount;

    public ObservableCollection<PosOrder> DisplayOrders { get; } = new();

    public OrderManagementViewModel(IOrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        
        // Load initial data
        LoadOrders();
    }

    [RelayCommand]
    public void LoadOrders()
    {
        try
        {
            var pagedResult = _orderService.GetAll();
            var allEntities = pagedResult.Items;

            var mappedOrders = allEntities.Select(MapToPosOrder).ToList();
            
            DisplayOrders.Clear();
            
            var activeStatuses = new[] { OrderStatus.Pending, OrderStatus.Preparing, OrderStatus.Ready };
            
            if (Filter == "Active")
            {
                foreach (var o in mappedOrders.Where(o => activeStatuses.Contains(o.Status)))
                    DisplayOrders.Add(o);
            }
            else
            {
                foreach (var o in mappedOrders.Where(o => !activeStatuses.Contains(o.Status)))
                    DisplayOrders.Add(o);
            }

            ActiveCount = mappedOrders.Count(o => activeStatuses.Contains(o.Status));
            HistoryCount = mappedOrders.Count(o => !activeStatuses.Contains(o.Status));
        }
        catch (Exception)
        {
            // Log or handle error
        }
    }

    [RelayCommand]
    public void LoadOrderPage(int pageNumber)
    {
        try
        {
            var paging = new PagingRequest { PageNumber = pageNumber, PageSize = 20 };
            var pagedResult = _orderService.GetAll(paging);
            
            // Add to display list or replace depending on UX needs
            foreach (var entity in pagedResult.Items)
            {
                DisplayOrders.Add(MapToPosOrder(entity));
            }
        }
        catch (Exception)
        {
            // Log or handle error
        }
    }

    private PosOrder MapToPosOrder(Order order)
    {
        var posOrder = new PosOrder
        {
            OrderId = $"ORD-{order.Id}",
            Status = MapToStatus(order.Status),
            CreatedAt = order.OrderDate,
            Subtotal = (double)order.SubTotal,
            Total = (double)order.TotalAmount,
            // Add other mappings as needed
        };

        if (order.OrderDetails != null)
        {
            foreach (var detail in order.OrderDetails)
            {
                posOrder.Items.Add(new OrderItem
                {
                    Product = detail.Product ?? new Product { Name = $"Product {detail.ProductId}" },
                    Quantity = detail.Quantity,
                    BasePrice = (double)detail.UnitPrice
                });
            }
        }

        return posOrder;
    }

    private OrderStatus MapToStatus(string status)
    {
        return status switch
        {
            "Pending" => OrderStatus.Pending,
            "Preparing" => OrderStatus.Preparing,
            "Ready" => OrderStatus.Ready,
            "Paid" => OrderStatus.Completed,
            "Cancelled" => OrderStatus.Voided,
            _ => OrderStatus.Pending
        };
    }

    [RelayCommand]
    private void SetFilter(string filterValue)
    {
        Filter = filterValue;
    }

    [RelayCommand]
    private void UpdateStatus(PosOrder order)
    {
        var currentStatus = order.Status;
        OrderStatus nextStatus = currentStatus;

        if (currentStatus == OrderStatus.Pending) nextStatus = OrderStatus.Preparing;
        else if (currentStatus == OrderStatus.Preparing) nextStatus = OrderStatus.Ready;
        else if (currentStatus == OrderStatus.Ready) nextStatus = OrderStatus.Completed;

        if (nextStatus != currentStatus)
        {
            try
            {
                int numericId = ExtractId(order.OrderId);
                _orderService.UpdateOrderStatus(numericId, MapFromStatus(nextStatus));
                LoadOrders(); // Refresh from service
            }
            catch (Exception) { /* Handle error */ }
        }
    }

    [RelayCommand]
    private void VoidOrder(PosOrder order)
    {
        try
        {
            int numericId = ExtractId(order.OrderId);
            _orderService.UpdateOrderStatus(numericId, "Cancelled");
            LoadOrders();
        }
        catch (Exception) { /* Handle error */ }
    }

    [RelayCommand]
    private void RefundOrder(PosOrder order)
    {
        // For demo, we just update status to something representing refund if available in service
        // or just local update if service doesn't support it yet
        try
        {
            int numericId = ExtractId(order.OrderId);
            _orderService.UpdateOrderStatus(numericId, "Cancelled"); // Or "Refunded" if supported
            LoadOrders();
        }
        catch (Exception) { /* Handle error */ }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        // Currently we don't have a bulk delete in service, so we do it one by one or just empty the local list for demo
        DisplayOrders.Clear();
        HistoryCount = 0;
    }

    partial void OnFilterChanged(string value) => LoadOrders();

    private void UpdateDisplayOrders() => LoadOrders();

    private int ExtractId(string orderId)
    {
        if (string.IsNullOrEmpty(orderId)) return 0;
        var parts = orderId.Split('-');
        if (parts.Length > 1 && int.TryParse(parts[1], out int id))
            return id;
        return 0;
    }

    private string MapFromStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.Preparing => "Preparing",
            OrderStatus.Ready => "Ready",
            OrderStatus.Completed => "Paid",
            OrderStatus.Voided => "Cancelled",
            _ => "Pending"
        };
    }
}
