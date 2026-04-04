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

    [ObservableProperty]
    private PosOrder _selectedOrderDetails;

    [ObservableProperty]
    private bool _isOrderDetailOpen;

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

    [ObservableProperty]
    private ObservableCollection<string> _statusFilterOptions = new();

    [ObservableProperty]
    private string _selectedStatusFilter;

    public Action ScrollToTopAction { get; set; }

    public ObservableCollection<PosOrder> DisplayOrders { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    public OrderManagementViewModel(IOrderService orderService, IProductService productService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        
        _currentPageHistory = 1;
        InitializeFilterOptions();
        LoadOrders();
    }

    private void InitializeFilterOptions()
    {
        if (Filter == "Active")
        {
            StatusFilterOptions = new ObservableCollection<string> { "All Active", "None", "Preparing", "Ready" };
        }
        else
        {
            StatusFilterOptions = new ObservableCollection<string> { "All History", "Paid", "Cancelled", "Refunded" };
        }
        
        _selectedStatusFilter = StatusFilterOptions.First();
        OnPropertyChanged(nameof(SelectedStatusFilter));
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        _currentPage = 1;
        CurrentPageHistory = 1;
        
        if (!IsLoadingMore)
        {
            LoadOrders();
        }
        
        if (Filter == "Active")
        {
            ScrollToTopAction?.Invoke();
        }
    }

    [RelayCommand]
    public void LoadOrders()
    {
        if (IsLoadingMore) return;
        
        try
        {
            IsLoadingMore = true;

            if (Filter == "Active")
            {
                var fulfillmentStatuses = SelectedStatusFilter == "All Active" || string.IsNullOrEmpty(SelectedStatusFilter)
                    ? new[] { "None", "Preparing", "Ready" }.ToList()
                    : new[] { SelectedStatusFilter }.ToList();

                _currentPage = 1;
                HasMoreData = true;

                var paging = new PagingRequest { PageNumber = _currentPage, PageSize = PageSize };
                var pagedResult = _orderService.GetPagedOrdersByStatuses(
                    paging, 
                    false, 
                    SearchStartDate?.DateTime, 
                    SearchEndDate?.DateTime, 
                    null, 
                    fulfillmentStatuses);
                
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
                var paymentStatuses = SelectedStatusFilter == "All History" || string.IsNullOrEmpty(SelectedStatusFilter)
                    ? new[] { "Paid", "Cancelled", "Refunded" }.ToList()
                    : new[] { SelectedStatusFilter }.ToList();
                    
                var paging = new PagingRequest { PageNumber = CurrentPageHistory, PageSize = PageSizeHistory };
                var pagedResult = _orderService.GetPagedOrdersByStatuses(
                    paging, 
                    true, 
                    SearchStartDate?.DateTime, 
                    SearchEndDate?.DateTime, 
                    paymentStatuses,
                    null);
                
                var mappedOrders = pagedResult.Items.Select(MapToPosOrder).ToList();
                
                DisplayOrders.Clear();
                foreach (var o in mappedOrders) DisplayOrders.Add(o);

                int historyCountFromDb = _orderService.GetCountByStatus(
                    SearchStartDate?.DateTime, 
                    SearchEndDate?.DateTime, 
                    paymentStatuses,
                    null);

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

            var fulfillmentStatuses = SelectedStatusFilter == "All Active" || string.IsNullOrEmpty(SelectedStatusFilter)
                ? new[] { "None", "Preparing", "Ready" }.ToList()
                : new[] { SelectedStatusFilter }.ToList();
                
            var pagedResult = _orderService.GetPagedOrdersByStatuses(
                new PagingRequest { PageNumber = _currentPage, PageSize = PageSize }, 
                false, 
                null, 
                null, 
                null,
                fulfillmentStatuses);
            
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
    private void ViewOrderDetail(PosOrder order)
    {
        if (order != null)
        {
            SelectedOrderDetails = order;
            IsOrderDetailOpen = true;
        }
    }

    [RelayCommand]
    private void CloseOrderDetail()
    {
        IsOrderDetailOpen = false;
        SelectedOrderDetails = null;
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
            PaymentStatus = MapToPaymentStatus(order.PaymentStatus),
            FulfillmentStatus = MapToFulfillmentStatus(order.FulfillmentStatus),
            CreatedAt = order.OrderDate,
        };

        if (order.OrderDetails != null)
        {
            double computedSubtotal = 0;
            foreach (var detail in order.OrderDetails)
            {
                var product = detail.Product ?? _productService.GetById(detail.ProductId) ?? new Product { Name = $"Product {detail.ProductId}" };
                
                posOrder.Items.Add(new OrderItem
                {
                    Product = product,
                    Quantity = detail.Quantity,
                    BasePrice = (double)detail.UnitPrice
                });
                
                computedSubtotal += (double)detail.UnitPrice * detail.Quantity;
            }
            
            posOrder.Subtotal = computedSubtotal;
            // Additional taxes or discounts could be handled here
            posOrder.Total = computedSubtotal;
        }

        return posOrder;
    }

    private PaymentStatus MapToPaymentStatus(string status)
    {
        return status switch
        {
            "Draft" => PaymentStatus.Draft,
            "PendingPayment" => PaymentStatus.PendingPayment,
            "Paid" => PaymentStatus.Paid,
            "Cancelled" => PaymentStatus.Cancelled,
            "Refunded" => PaymentStatus.Refunded,
            _ => PaymentStatus.Draft
        };
    }

    private FulfillmentStatus MapToFulfillmentStatus(string status)
    {
        return status switch
        {
            "None" => FulfillmentStatus.None,
            "Preparing" => FulfillmentStatus.Preparing,
            "Ready" => FulfillmentStatus.Ready,
            "Completed" => FulfillmentStatus.Completed,
            _ => FulfillmentStatus.None
        };
    }

    [RelayCommand]
    private void SetFilter(string filterValue)
    {
        Filter = filterValue;
    }

    [RelayCommand]
    private void UpdateOrderStatus(PosOrder order)
    {
        var currentStatus = order.FulfillmentStatus;
        FulfillmentStatus nextStatus = currentStatus;

        if (currentStatus == FulfillmentStatus.None) nextStatus = FulfillmentStatus.Preparing;
        else if (currentStatus == FulfillmentStatus.Preparing) nextStatus = FulfillmentStatus.Ready;
        else if (currentStatus == FulfillmentStatus.Ready) nextStatus = FulfillmentStatus.Completed;

        if (nextStatus != currentStatus)
        {
            try
            {
                int numericId = ExtractId(order.OrderId);
                _orderService.UpdateFulfillmentStatus(numericId, nextStatus.ToString());
                LoadOrders(); // Refresh from service
            }
            catch (Exception) { /* Handle error */ }
        }
    }

    [RelayCommand]
    private void CancelOrder(PosOrder order)
    {
        try
        {
            int numericId = ExtractId(order.OrderId);
            _orderService.UpdatePaymentStatus(numericId, "Cancelled");
            LoadOrders();
        }
        catch (Exception) { /* Handle error */ }
    }

    [RelayCommand]
    private void RefundOrder(PosOrder order)
    {
        try
        {
            int numericId = ExtractId(order.OrderId);
            _orderService.UpdatePaymentStatus(numericId, "Refunded"); 
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

    partial void OnFilterChanged(string value)
    {
        InitializeFilterOptions();
        LoadOrders();
    }


    private void UpdateDisplayOrders() => LoadOrders();

    private int ExtractId(string orderId)
    {
        if (string.IsNullOrEmpty(orderId)) return 0;
        var parts = orderId.Split('-');
        if (parts.Length > 1 && int.TryParse(parts[1], out int id))
            return id;
        return 0;
    }

    // Helper converter maps are eliminated to use direct Enum ToString serialization inside UpdateOrderStatus.

    private void LoadCounts()
    {
        try
        {
            var activeStatuses = new[] { "None", "Preparing", "Ready" }.ToList();
            ActiveCount = _orderService.GetCountByStatus(null, null, null, activeStatuses);
            
            var historyStatuses = new[] { "Paid", "Cancelled", "Refunded" }.ToList();
            HistoryCount = _orderService.GetCountByStatus(
                SearchStartDate?.DateTime, 
                SearchEndDate?.DateTime, 
                historyStatuses, 
                null);
        }
        catch { /* ignored */ }
    }
}
