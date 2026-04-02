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
    private readonly IProductService _productService;


    [ObservableProperty]
    private string _filter = "Active"; // "Active" or "History"

    [ObservableProperty]
    private int _activeCount;

    [ObservableProperty]
    private int _historyCount;

    [ObservableProperty]
    private bool _isLoadingMore;

    [ObservableProperty]
    private bool _hasMoreData = true;

    // Infinite scroll for active tab
    private int _currentPage = 1;
    private const int PageSize = 20;

    // Standard paging for history tab
    private const int PageSizeHistory = 4;
    
    [ObservableProperty]
    private int _currentPageHistory = 1;

    [ObservableProperty]
    private int _totalPagesHistory;

    [ObservableProperty]
    private string _pagingSummary = "Showing 0-0 in 0 items";

    [ObservableProperty]
    private DateTimeOffset? _searchStartDate;

    [ObservableProperty]
    private DateTimeOffset? _searchEndDate;

    public ObservableCollection<PosOrder> DisplayOrders { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    public OrderManagementViewModel(IOrderService orderService, IProductService productService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        
        _currentPageHistory = 1;
        LoadOrders();
    }

    [RelayCommand]
    public void LoadOrders()
    {
        if (IsLoadingMore) return;
        
        try
        {
            IsLoadingMore = true;

            var activeStatuses = new[] { "Pending", "Preparing", "Ready" };

            if (Filter == "Active")
            {
                _currentPage = 1;
                HasMoreData = true;

                var paging = new PagingRequest { PageNumber = _currentPage, PageSize = PageSize };
                var pagedResult = _orderService.GetPagedOrdersByStatuses(paging, false, SearchStartDate?.DateTime, SearchEndDate?.DateTime, activeStatuses);
                
                var mappedOrders = pagedResult.Items.Select(MapToPosOrder).ToList();
                
                DisplayOrders.Clear();
                foreach (var o in mappedOrders) DisplayOrders.Add(o);

                HasMoreData = _currentPage < pagedResult.Pagination.TotalPages;
                PageNumbers.Clear();
                PagingSummary = "";
            }
            else
            {
                // History Paging
                var historyStatuses = new[] { "Paid", "Cancelled" };
                var paging = new PagingRequest { PageNumber = CurrentPageHistory, PageSize = PageSizeHistory };
                var pagedResult = _orderService.GetPagedOrdersByStatuses(
                    paging, 
                    true, 
                    SearchStartDate?.DateTime, 
                    SearchEndDate?.DateTime, 
                    historyStatuses);
                
                var mappedOrders = pagedResult.Items.Select(MapToPosOrder).ToList();
                
                DisplayOrders.Clear();
                foreach (var o in mappedOrders) DisplayOrders.Add(o);

                int historyCountFromDb = _orderService.GetCountByStatus(
                    SearchStartDate?.DateTime, 
                    SearchEndDate?.DateTime, 
                    "Paid", "Cancelled");

                TotalPagesHistory = (int)Math.Ceiling(historyCountFromDb / (double)PageSizeHistory);
                if (TotalPagesHistory < 1) TotalPagesHistory = 1;

                PageNumbers.Clear();
                for (int i = 1; i <= TotalPagesHistory; i++)
                    PageNumbers.Add(i);

                var start = historyCountFromDb == 0 ? 0 : ((CurrentPageHistory - 1) * PageSizeHistory) + 1;
                var end = Math.Min(start + PageSizeHistory - 1, historyCountFromDb);
                PagingSummary = $"Showing {start}-{end} in {historyCountFromDb} items";
            }

            LoadCounts();
        }
        catch (Exception)
        {
            // Log or handle error
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    [RelayCommand]
    public void LoadMoreOrders()
    {
        if (Filter != "Active") return; // Only used for Active tab
        if (IsLoadingMore || !HasMoreData) return;

        try
        {
            IsLoadingMore = true;
            _currentPage++;

            var activeStatuses = new[] { "Pending", "Preparing", "Ready" };
            var pagedResult = _orderService.GetPagedOrdersByStatuses(
                new PagingRequest { PageNumber = _currentPage, PageSize = PageSize }, 
                false, 
                null, 
                null, 
                activeStatuses);
            
            var mappedOrders = pagedResult.Items.Select(MapToPosOrder).ToList();
            
            foreach (var o in mappedOrders)
                DisplayOrders.Add(o);

            HasMoreData = _currentPage < pagedResult.Pagination.TotalPages;
        }
        catch (Exception)
        {
            // Log or handle error
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    [RelayCommand]
    private void SearchByDateRange()
    {
        CurrentPageHistory = 1;
        LoadOrders();
    }

    [RelayCommand]
    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPagesHistory) return;
        CurrentPageHistory = page;
        LoadOrders();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPageHistory < TotalPagesHistory)
        {
            CurrentPageHistory++;
            LoadOrders();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPageHistory > 1)
        {
            CurrentPageHistory--;
            LoadOrders();
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
                var product = detail.Product ?? _productService.GetById(detail.ProductId) ?? new Product { Name = $"Product {detail.ProductId}" };
                
                posOrder.Items.Add(new OrderItem
                {
                    Product = product,
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

    private void LoadCounts()
    {
        try
        {
            ActiveCount = _orderService.GetCountByStatus(null, null, "Pending", "Preparing", "Ready");
            
            // History count uses the date range bounds
            HistoryCount = _orderService.GetCountByStatus(
                SearchStartDate?.DateTime, 
                SearchEndDate?.DateTime, 
                "Paid", "Cancelled");
        }
        catch { /* ignored */ }
    }
}
