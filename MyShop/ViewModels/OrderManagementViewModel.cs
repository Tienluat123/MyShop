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
    private PosOrder? _selectedOrderDetails;

    [ObservableProperty]
    private bool _isOrderDetailOpen;

    // Infinite scroll for active tab
    private int _currentPage = 1;
    private const int PageSize = 20;

    // Standard paging for history tab
    private const int PageSizeHistory = 10;

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
    private string _selectedStatusFilter = string.Empty;
    
    [ObservableProperty]
    private bool _isEditingDetail;

    [ObservableProperty]
    private string _editTableNumber = string.Empty;

    [ObservableProperty]
    private string _editNotes = string.Empty;

    [ObservableProperty]
    private PaymentStatus _editPaymentStatus;

    [ObservableProperty]
    private FulfillmentStatus _editFulfillmentStatus;

    [ObservableProperty]
    private decimal _editSubtotal;

    [ObservableProperty]
    private decimal _editTax;

    [ObservableProperty]
    private decimal _editDiscount;

    [ObservableProperty]
    private double _editManualAdjustment;

    [ObservableProperty]
    private string _editManualAdjustmentReason = "";

    [ObservableProperty]
    private decimal _editTotal;

    public bool CanEditItems => SelectedOrderDetails != null && 
                                SelectedOrderDetails.PaymentStatus != PaymentStatus.Paid && 
                                SelectedOrderDetails.PaymentStatus != PaymentStatus.Refunded;

    public bool IsPaid => SelectedOrderDetails?.PaymentStatus == PaymentStatus.Paid;

    public ObservableCollection<PaymentStatus> PaymentStatusOptions { get; } = new(Enum.GetValues<PaymentStatus>());
    public ObservableCollection<FulfillmentStatus> FulfillmentStatusOptions { get; } = new(Enum.GetValues<FulfillmentStatus>());

    public Action? ScrollToTopAction { get; set; }
    public Action? CloseOrderDetailAction { get; set; }

    public ObservableCollection<PosOrder> DisplayOrders { get; } = new();
    public ObservableCollection<OrderItem> EditableItems { get; } = new();
    public ObservableCollection<Product> AllProducts { get; } = new();
    public ObservableCollection<int> PageNumbers { get; } = new();

    public OrderManagementViewModel(IOrderService orderService, IProductService productService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));

        _currentPageHistory = 1;
        InitializeFilterOptions();
        _ = LoadOrders();
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

        SelectedStatusFilter = StatusFilterOptions.First();
    }

    partial void OnSelectedStatusFilterChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        _currentPage = 1;
        CurrentPageHistory = 1;

        _ = LoadOrders();

        if (Filter == "Active")
        {
            ScrollToTopAction?.Invoke();
        }
    }

    [RelayCommand]
    public async Task LoadOrders()
    {
        if (IsLoadingMore) return;

        try
        {
            IsLoadingMore = true;

            if (Filter == "Active")
            {
                var paging = new PagingRequest { PageNumber = _currentPage, PageSize = PageSize };
                var pagedResult = await _orderService.GetPagedActiveOrders(paging, SelectedStatusFilter);

                var mappedOrders = await Task.WhenAll(pagedResult.Items.Select(MapToPosOrder));

                DisplayOrders.Clear();
                foreach (var o in mappedOrders) DisplayOrders.Add(o);

                HasMoreData = _currentPage < pagedResult.Pagination.TotalPages;
                PageNumbers.Clear();
                PagingSummary = "";
            }
            else
            {
                // History Paging
                var paging = new PagingRequest { PageNumber = CurrentPageHistory, PageSize = PageSizeHistory };
                var pagedResult = await _orderService.GetPagedHistoryOrders(
                    paging,
                    SearchStartDate?.DateTime,
                    SearchEndDate?.DateTime,
                    SelectedStatusFilter);

                var mappedOrders = await Task.WhenAll(pagedResult.Items.Select(MapToPosOrder));

                DisplayOrders.Clear();
                foreach (var o in mappedOrders) DisplayOrders.Add(o);

                int historyCountFromDb = pagedResult.Pagination.TotalItems;

                TotalPagesHistory = (int)Math.Ceiling(historyCountFromDb / (double)PageSizeHistory);
                if (TotalPagesHistory < 1) TotalPagesHistory = 1;

                PageNumbers.Clear();
                for (int i = 1; i <= TotalPagesHistory; i++)
                    PageNumbers.Add(i);

                var start = historyCountFromDb == 0 ? 0 : ((CurrentPageHistory - 1) * PageSizeHistory) + 1;
                var end = Math.Min(start + PageSizeHistory - 1, historyCountFromDb);
                PagingSummary = $"Showing {start}-{end} in {historyCountFromDb} items";
            }

            await LoadCounts();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading orders: {ex.Message}");
            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    [RelayCommand]
    public async Task LoadMoreOrders()
    {
        if (Filter != "Active") return; // Only used for Active tab
        if (IsLoadingMore || !HasMoreData) return;

        try
        {
            IsLoadingMore = true;
            _currentPage++;

            var pagedResult = await _orderService.GetPagedActiveOrders(
                new PagingRequest { PageNumber = _currentPage, PageSize = PageSize },
                SelectedStatusFilter);

            var mappedOrders = await Task.WhenAll(pagedResult.Items.Select(MapToPosOrder));

            foreach (var o in mappedOrders)
                DisplayOrders.Add(o);

            HasMoreData = _currentPage < pagedResult.Pagination.TotalPages;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading more orders: {ex.Message}");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    [RelayCommand]
    private async Task SearchByDateRange()
    {
        CurrentPageHistory = 1;
        await LoadOrders();
    }

    [RelayCommand]
    private async Task ClearDateFilter()
    {
        SearchStartDate = null;
        SearchEndDate = null;
        CurrentPageHistory = 1;
        await LoadOrders();
    }

    public async Task SelectOrder(Order order)
    {
        if (order == null) return;
        
        // 1. Đảm bảo tải đầy đủ chi tiết đơn hàng (các món ăn) từ DB
        if (order.OrderDetails == null || !order.OrderDetails.Any())
        {
             var fullOrder = await _orderService.GetById(order.Id);
             if (fullOrder != null) {
                 order.OrderDetails = fullOrder.OrderDetails;
                 order.Customer = fullOrder.Customer; // Đảm bảo tải cả khách hàng
             }
        }

        // 2. Chuyển đổi sang định dạng PosOrder để hiển thị được danh sách món ăn
        SelectedOrderDetails = await MapToPosOrder(order);
        IsOrderDetailOpen = true;
        IsEditingDetail = false;
        
        OnPropertyChanged(nameof(SelectedOrderDetails));
    }

    [RelayCommand]
    private void ViewOrderDetail(PosOrder order)
    {
        if (order != null)
        {
            SelectedOrderDetails = order;
            IsOrderDetailOpen = true;
            IsEditingDetail = false; // Reset to view mode
        }
    }

    [RelayCommand]
    private void EditOrderDetail()
    {
        if (SelectedOrderDetails == null) return;

        EditTableNumber = SelectedOrderDetails.Table ?? string.Empty;
        EditNotes = SelectedOrderDetails.Notes ?? string.Empty;
        EditPaymentStatus = SelectedOrderDetails.PaymentStatus;
        EditFulfillmentStatus = SelectedOrderDetails.FulfillmentStatus;
        EditManualAdjustment = (double)SelectedOrderDetails.ManualAdjustment;
        EditManualAdjustmentReason = SelectedOrderDetails.ManualAdjustmentReason ?? string.Empty;

        // Clone items for editing
        EditableItems.Clear();
        foreach (var item in SelectedOrderDetails.Items)
        {
            EditableItems.Add(new OrderItem
            {
                Product = item.Product,
                BasePrice = item.BasePrice,
                Quantity = item.Quantity,
                DrinkOptions = item.DrinkOptions // Shallow copy of options is fine for now
            });
        }

        UpdateEditTotals();
        IsEditingDetail = true;
        OnPropertyChanged(nameof(CanEditItems));
        OnPropertyChanged(nameof(IsPaid));
    }

    private void UpdateEditTotals()
    {
        EditSubtotal = EditableItems.Sum(i => i.TotalPrice);
        EditTax = EditSubtotal * 0.0875m; // Synced with POS TAX_RATE
        // Keep existing discount logic if any, or just use the one from modal for now
        EditDiscount = SelectedOrderDetails?.Discount ?? 0; 
        EditTotal = Math.Max(0, EditSubtotal + EditTax - EditDiscount - (decimal)EditManualAdjustment);
    }

    [RelayCommand]
    private void IncreaseItemQty(OrderItem item)
    {
        if (!CanEditItems) return;
        item.Quantity++;
        UpdateEditTotals();
    }

    [RelayCommand]
    private void DecreaseItemQty(OrderItem item)
    {
        if (!CanEditItems) return;
        if (item.Quantity > 1)
        {
            item.Quantity--;
            UpdateEditTotals();
        }
    }

    [RelayCommand]
    private void RemoveItemFromEdit(OrderItem item)
    {
        if (!CanEditItems) return;
        EditableItems.Remove(item);
        UpdateEditTotals();
    }

    [RelayCommand]
    private void AddProductToEdit(Product product)
    {
        if (!CanEditItems || product == null) return;
        
        var existing = EditableItems.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            EditableItems.Add(new OrderItem
            {
                Product = product,
                BasePrice = product.Price,
                Quantity = 1
            });
        }
        UpdateEditTotals();
    }

    partial void OnEditManualAdjustmentChanged(double value) => UpdateEditTotals();

    [RelayCommand]
    private void CancelEditOrderDetail()
    {
        IsEditingDetail = false;
    }

    [RelayCommand]
    private async Task SaveOrderDetail()
    {
        if (SelectedOrderDetails == null) return;

        try
        {
            int numericId = ExtractId(SelectedOrderDetails.OrderId);
            var order = await _orderService.GetById(numericId);
            if (order == null) return;

            order.TableNumber = EditTableNumber;
            order.Notes = EditNotes;
            order.PaymentStatus = EditPaymentStatus.ToString();
            order.FulfillmentStatus = EditFulfillmentStatus.ToString();
            order.ManualAdjustment = (decimal)EditManualAdjustment;
            order.ManualAdjustmentReason = EditManualAdjustmentReason;

            // Sync items back to OrderDetails
            if (CanEditItems || IsPaid) // Allow adjustments for paid too
            {
                var newDetails = new List<OrderDetail>();
                foreach (var item in EditableItems)
                {
                    newDetails.Add(new OrderDetail
                    {
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.BasePrice,
                        Notes = item.DrinkOptions?.Notes
                    });
                }
                
                // Recalculate financial fields
                order.SubTotal = EditSubtotal;
                order.TaxAmount = EditTax;
                order.DiscountAmount = EditDiscount;
                order.TotalAmount = EditTotal;

                await _orderService.UpdateWithDetails(order, newDetails);
            }
            else
            {
                await _orderService.Update(order);
            }
            IsEditingDetail = false;
            
            await LoadOrders();
            
            // Refresh detail view
            var updated = DisplayOrders.FirstOrDefault(o => o.OrderId == SelectedOrderDetails.OrderId);
            if (updated != null) SelectedOrderDetails = updated;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Save failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CloseOrderDetail()
    {
        IsOrderDetailOpen = false;
        SelectedOrderDetails = null;
        IsEditingDetail = false;
        
        // Thực thi lệnh đóng từ bên ngoài (nếu có)
        CloseOrderDetailAction?.Invoke();
    }

    [RelayCommand]
    private async Task GoToPage(int page)
    {
        if (page < 1 || page > TotalPagesHistory) return;
        CurrentPageHistory = page;
        ScrollToTopAction?.Invoke();
        await LoadOrders();
    }

    [RelayCommand]
    private async Task NextPage()
    {
        if (CurrentPageHistory < TotalPagesHistory)
        {
            CurrentPageHistory++;
            ScrollToTopAction?.Invoke();
            await LoadOrders();
        }
    }

    [RelayCommand]
    private async Task PreviousPage()
    {
        if (CurrentPageHistory > 1)
        {
            CurrentPageHistory--;
            ScrollToTopAction?.Invoke();
            await LoadOrders();
        }
    }

    private async Task<PosOrder> MapToPosOrder(Order order)
    {
        System.Diagnostics.Debug.WriteLine($"[OrderVM] Mapping Order ID={order.Id}, CustomerId={order.CustomerId}, HasCustomer={order.Customer != null}, CustomerName='{order.Customer?.Name}'");
        var posOrder = new PosOrder
        {
            OrderId = $"ORD-{order.Id}",
            PaymentStatus = MapToPaymentStatus(order.PaymentStatus),
            FulfillmentStatus = MapToFulfillmentStatus(order.FulfillmentStatus),
            CreatedAt = ToLocalOrderDate(order.OrderDate),
            Type = order.Type,
            Table = order.TableNumber,
            CustomerId = order.CustomerId?.ToString(),
            CustomerName = order.Customer?.Name ?? "Guest",
            Notes = order.Notes,
            Tax = order.TaxAmount,
            Discount = order.DiscountAmount,
            ManualAdjustment = order.ManualAdjustment,
            ManualAdjustmentReason = order.ManualAdjustmentReason ?? string.Empty,
            Subtotal = order.SubTotal,
            Total = order.TotalAmount
        };

        if (order.OrderDetails != null)
        {
            foreach (var detail in order.OrderDetails)
            {
                var product = detail.Product
                    ?? await _productService.GetById(detail.ProductId)
                    ?? new Product { Name = $"Product {detail.ProductId}" };

                posOrder.Items.Add(new OrderItem
                {
                    Product = product,
                    Quantity = detail.Quantity,
                    BasePrice = detail.UnitPrice
                });
            }
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

    private static DateTime ToLocalOrderDate(DateTime orderDate)
    {
        var utcOrderDate = orderDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(orderDate, DateTimeKind.Utc)
            : orderDate.ToUniversalTime();

        return utcOrderDate.ToLocalTime();
    }

    [RelayCommand]
    private void SetFilter(string filterValue)
    {
        Filter = filterValue;
    }

    [RelayCommand]
    private async Task UpdateOrderStatus(PosOrder order)
    {
        var currentStatus = order.FulfillmentStatus;
        FulfillmentStatus nextStatus = currentStatus;

        if (currentStatus == FulfillmentStatus.None) nextStatus = FulfillmentStatus.Preparing;
        else if (currentStatus == FulfillmentStatus.Preparing) nextStatus = FulfillmentStatus.Ready;
        else if (currentStatus == FulfillmentStatus.Ready)
        {
            if (order.PaymentStatus != PaymentStatus.Paid)
            {
                // Safety guard: Prevents Ready -> Completed if not paid
                return;
            }
            nextStatus = FulfillmentStatus.Completed;
        }

        if (nextStatus != currentStatus)
        {
            try
            {
                int numericId = ExtractId(order.OrderId);
                await _orderService.UpdateFulfillmentStatus(numericId, nextStatus.ToString());
                await LoadOrders(); // Refresh from service
            }
            catch (Exception) { /* Handle error */ }
        }
    }

    [RelayCommand]
    private async Task CancelOrder(PosOrder order)
    {
        try
        {
            int numericId = ExtractId(order.OrderId);
            await _orderService.UpdatePaymentStatus(numericId, "Cancelled");
            await LoadOrders();
        }
        catch (Exception) { /* Handle error */ }
    }

    [RelayCommand]
    private async Task RefundOrder(PosOrder order)
    {
        try
        {
            int numericId = ExtractId(order.OrderId);
            await _orderService.UpdatePaymentStatus(numericId, "Refunded");
            await LoadOrders();
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
        _ = LoadOrders();
    }


    private void UpdateDisplayOrders() => _ = LoadOrders();

    private int ExtractId(string orderId)
    {
        if (string.IsNullOrEmpty(orderId)) return 0;
        var parts = orderId.Split('-');
        if (parts.Length > 1 && int.TryParse(parts[1], out int id))
            return id;
        return 0;
    }

    // Helper converter maps are eliminated to use direct Enum ToString serialization inside UpdateOrderStatus.

    private async Task LoadCounts()
    {
        try
        {
            ActiveCount = await _orderService.GetActiveCount();

            HistoryCount = await _orderService.GetHistoryCount(
                SearchStartDate?.DateTime,
                SearchEndDate?.DateTime);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading counts: {ex.Message}");
        }
    }
}
