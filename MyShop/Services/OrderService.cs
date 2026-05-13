using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace MyShop.Services;

/// <summary>
/// Interface cho Order Service
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Lấy tất cả đơn hàng
    /// </summary>
    Task<PagedResult<Order>> GetAll(PagingRequest? paging = null);

    /// <summary>
    /// Lấy đơn hàng theo ID
    /// </summary>
    Task<Order?> GetById(int id);

    /// <summary>
    /// Lấy đơn hàng của khách hàng
    /// </summary>
    Task<List<Order>> GetByCustomerId(int customerId);

    /// <summary>
    /// Lấy đơn hàng theo thuộc tính
    /// </summary>
    Task<List<Order>> GetByPaymentStatus(string status);

    /// <summary>
    /// Lấy đơn hàng chưa thanh toán
    /// </summary>
    Task<List<Order>> GetUnpaidOrders();

    /// <summary>
    /// Tính doanh thu trong khoảng ngày
    /// </summary>
    Task<decimal> GetRevenue(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Thêm đơn hàng mới
    /// </summary>
    Task<Order> Add(Order order);

    /// <summary>
    /// Cập nhật đơn hàng
    /// </summary>
    Task Update(Order order);

    /// <summary>
    /// Cập nhật đơn hàng kèm theo danh sách chi tiết (thay thế toàn bộ chi tiết cũ)
    /// </summary>
    Task UpdateWithDetails(Order order, List<OrderDetail> details);

    /// <summary>
    /// Xóa đơn hàng
    /// </summary>
    Task Delete(int id);

    /// <summary>
    /// Cập nhật trạng thái thanh toán
    /// </summary>
    Task UpdatePaymentStatus(int orderId, string newStatus);

    /// <summary>
    /// Cập nhật trạng thái giao/nhận hàng
    /// </summary>
    Task UpdateFulfillmentStatus(int orderId, string newStatus);

    /// <summary>
    /// Lấy danh sách đơn hàng được lọc và sắp xếp, hỗ trợ phân trang kép
    /// </summary>
    Task<PagedResult<Order>> GetPagedOrdersByStatuses(PagingRequest paging, bool sortDescending, DateTime? startDate = null, DateTime? endDate = null, List<string>? paymentStatuses = null, List<string>? fulfillmentStatuses = null, int? userId = null);

    /// <summary>
    /// Đếm số lượng đơn hàng theo danh sách trạng thái
    /// </summary>
    Task<int> GetCountByStatus(DateTime? startDate = null, DateTime? endDate = null, List<string>? paymentStatuses = null, List<string>? fulfillmentStatuses = null, int? userId = null);

    /// <summary>
    /// Lấy danh sách đơn hàng Active (chưa hoàn tất và chưa hủy)
    /// </summary>
    Task<PagedResult<Order>> GetPagedActiveOrders(PagingRequest paging, string? statusFilter = null);

    /// <summary>
    /// Đếm số lượng đơn hàng Active
    /// </summary>
    Task<int> GetActiveCount();

    /// <summary>
    /// Lấy danh sách đơn hàng History (đã hoàn tất/thanh toán, hoặc đã hủy/hoàn tiền)
    /// </summary>
    Task<PagedResult<Order>> GetPagedHistoryOrders(PagingRequest paging, DateTime? startDate = null, DateTime? endDate = null, string? statusFilter = null);

    /// <summary>
    /// Đếm số lượng đơn hàng History
    /// </summary>
    Task<int> GetHistoryCount(DateTime? startDate = null, DateTime? endDate = null);

    Task<IEnumerable<Order>> GetOrdersByDateRange(DateTime from, DateTime to);    
    Task<IEnumerable<Order>> GetAllOrdersWithDetails();    
    Task<int> GetTodayOrderCount();    
    Task<decimal> GetTodayRevenue();
    Task<List<(string Label, decimal Value)>> GetRevenueByDateInMonth(int month, int year);
    Task<List<(string Name, int Quantity)>> GetTopSellingProducts(int limit = 5);
    Task<IEnumerable<Order>> GetRecentOrders(int limit = 3);
}

/// <summary>
/// Order Service Implementation
/// </summary>
public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<User> _userRepository;

    public OrderService(
        IRepository<Order> orderRepository,
        IRepository<Customer> customerRepository,
        IRepository<User> userRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
   
    /// <summary>
    /// Lấy tất cả đơn hàng (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<Order>> GetAll(PagingRequest? paging = null)
    {
        try
        {
            return await _orderRepository.GetAllAsync(paging);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load orders", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng theo ID (Bất đồng bộ)
    /// </summary>
    public async Task<Order?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        return await _orderRepository.ExecuteAsync(async context =>
        {
            return await context.Set<Order>()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);
        }, "GetByIdWithDetails");
    }

    /// <summary>
    /// Lấy đơn hàng của khách hàng (Bất đồng bộ)
    /// </summary>
    public async Task<List<Order>> GetByCustomerId(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be greater than 0", nameof(customerId));

        try
        {
            var items = await _orderRepository.GetAsync(o => o.CustomerId == customerId);
            return items;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load customer orders {customerId}", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng theo PaymentStatus (Bất đồng bộ)
    /// </summary>
    public async Task<List<Order>> GetByPaymentStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required", nameof(status));

        try
        {
            var items = await _orderRepository.GetAsync(o => o.PaymentStatus == status);
            return items;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load orders by payment status '{status}'", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng chưa thanh toán (Bất đồng bộ)
    /// </summary>
    public async Task<List<Order>> GetUnpaidOrders()
    {
        try
        {
            return await GetByPaymentStatus("PendingPayment");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load unpaid orders", ex);
        }
    }

    /// <summary>
    /// Tính doanh thu trong khoảng ngày (Bất đồng bộ)
    /// </summary>
    public async Task<decimal> GetRevenue(DateTime startDate, DateTime endDate)
    {
        try
        {
            // Use ExecuteAsync to properly handle context lifetime
            var result = await _orderRepository.ExecuteAsync(async context =>
            {
                var revenueSum = await context.Set<Order>()
                    .Where(o =>
                        o.OrderDate >= startDate &&
                        o.OrderDate <= endDate &&
                        o.PaymentStatus == "Paid")
                    .SumAsync(o => (decimal?)o.TotalAmount);
                return revenueSum ?? 0;
            }, "GetRevenue");

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to calculate revenue", ex);
        }
    }

    /// <summary>
    /// Thêm đơn hàng mới (Bất đồng bộ)
    /// </summary>
    public async Task<Order> Add(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        // CustomerId is nullable, but if provided it should be positive (or the DB will fail)
        if (order.CustomerId.HasValue && order.CustomerId.Value < 0)
            throw new ArgumentException("Invalid customer ID", nameof(order.CustomerId));

        if (order.UserId <= 0)
            throw new ArgumentException("A valid employee is required", nameof(order.UserId));

        if (order.OrderDetails?.Any() == true)
        {
            order.SubTotal = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
            
            var discountBase = order.SubTotal - order.DiscountAmount - order.ManualAdjustment;
            order.TotalAmount = discountBase + order.TaxAmount;
        }

        if (order.TotalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative", nameof(order.TotalAmount));

        if (order.CustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId.Value);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {order.CustomerId} does not exist");
        }

        var user = await _userRepository.GetByIdAsync(order.UserId);
        if (user == null)
            throw new InvalidOperationException($"Employee with ID {order.UserId} does not exist");

        // Ensure date is UTC to prevent Npgsql timezone shifts
        order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);

        try
        {
            return await _orderRepository.AddAsync(order);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to add order", ex);
        }
    }

    /// <summary>
    /// Cập nhật đơn hàng (Bất đồng bộ)
    /// </summary>
    public async Task Update(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.Id <= 0)
            throw new ArgumentException("Invalid order ID", nameof(order.Id));

        try
        {
            // Ensure date remains UTC
            order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);
            await _orderRepository.UpdateAsync(order);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update order", ex);
        }
    }

    public async Task UpdateWithDetails(Order order, List<OrderDetail> details)
    {
        if (order == null || order.Id <= 0)
            throw new ArgumentException("Invalid order");

        await _orderRepository.ExecuteAsync(async context =>
        {
            // 1. Sync the Order header
            var existingOrder = await context.Set<Order>()
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (existingOrder == null) return false;

            // Update scalar properties
            context.Entry(existingOrder).CurrentValues.SetValues(order);
            context.Entry(existingOrder).Property(o => o.OrderDate).IsModified = false;

            // 2. Sync Details (Upsert pattern)
            // Remove missing details
            var detailIdsToKeep = details.Where(d => d.Id > 0).Select(d => d.Id).ToList();
            var detailsToRemove = existingOrder.OrderDetails
                .Where(d => !detailIdsToKeep.Contains(d.Id))
                .ToList();

            foreach (var d in detailsToRemove)
                context.Set<OrderDetail>().Remove(d);

            // Update or Add details
            foreach (var detail in details)
            {
                var existingDetail = existingOrder.OrderDetails
                    .FirstOrDefault(d => d.Id > 0 && d.Id == detail.Id);

                if (existingDetail != null)
                {
                    // Update existing
                    context.Entry(existingDetail).CurrentValues.SetValues(detail);
                }
                else
                {
                    // Add new
                    detail.OrderId = order.Id;
                    detail.Id = 0;
                    existingOrder.OrderDetails.Add(detail);
                }
            }

            await context.SaveChangesAsync();
            return true;
        }, "UpdateWithDetails");
    }

    /// <summary>
    /// Xóa đơn hàng (Bất đồng bộ)
    /// </summary>
    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid order ID", nameof(id));

        try
        {
            await _orderRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete order with ID {id}", ex);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái thanh toán (Bất đồng bộ)
    /// </summary>
    public async Task UpdatePaymentStatus(int orderId, string newStatus)
    {
        if (orderId <= 0)
            throw new ArgumentException("Invalid order ID", nameof(orderId));

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("New payment status is required", nameof(newStatus));

        try
        {
            var order = await GetById(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} was not found");

            if (order.PaymentStatus == "Paid" && newStatus != "Paid" && newStatus != "Refunded")
                throw new InvalidOperationException("Paid orders cannot be changed");

            if (order.PaymentStatus == "Cancelled")
                throw new InvalidOperationException("Canceled orders cannot be changed");

            order.PaymentStatus = newStatus;
            await Update(order);
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"System error while updating payment status", ex);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái fulfillment (Bất đồng bộ)
    /// </summary>
    public async Task UpdateFulfillmentStatus(int orderId, string newStatus)
    {
        if (orderId <= 0)
            throw new ArgumentException("Invalid order ID", nameof(orderId));

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("New fulfillment status is required", nameof(newStatus));

        try
        {
            var order = await GetById(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} was not found");

            order.FulfillmentStatus = newStatus;
            await Update(order);
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"System error while updating fulfillment status", ex);
        }
    }

    /// <summary>
    /// Đếm số lượng đơn hàng theo danh sách trạng thái (Bất đồng bộ)
    /// </summary>
    public async Task<int> GetCountByStatus(DateTime? startDate = null, DateTime? endDate = null, List<string>? paymentStatuses = null, List<string>? fulfillmentStatuses = null, int? userId = null)
    {
        try
        {
            // Build filter expression
            Expression<Func<Order, bool>> filter = o => true;

            if (userId != null)
                filter = CombineFilters(filter, o => o.UserId == userId.Value);

            if (paymentStatuses != null && paymentStatuses.Count > 0)
                filter = CombineFilters(filter, o => paymentStatuses.Contains(o.PaymentStatus));

            if (fulfillmentStatuses != null && fulfillmentStatuses.Count > 0)
                filter = CombineFilters(filter, o => fulfillmentStatuses.Contains(o.FulfillmentStatus));

            if (startDate.HasValue)
            {
                var startDateValue = startDate.Value;
                filter = CombineFilters(filter, o => o.OrderDate >= startDateValue);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                filter = CombineFilters(filter, o => o.OrderDate <= endOfDay);
            }

            return await _orderRepository.CountAsync(filter);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to count orders", ex);
        }
    }

    /// <summary>
    /// Lấy danh sách đơn hàng được lọc và sắp xếp, hỗ trợ phân trang kép (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<Order>> GetPagedOrdersByStatuses(PagingRequest paging, bool sortDescending, DateTime? startDate = null, DateTime? endDate = null, List<string>? paymentStatuses = null, List<string>? fulfillmentStatuses = null, int? userId = null)
    {
        if (paging == null) throw new ArgumentNullException(nameof(paging));
        paging.Validate();
        var sortedPaging = new PagingRequest
        {
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize,
            SearchText = paging.SearchText,
            Filters = paging.Filters,
            SortBy = sortDescending ? "OrderDate DESC" : "OrderDate ASC"
        };

        try
        {
            // Build filter expression
            Expression<Func<Order, bool>> filter = o => true; // Start with always true

            if (userId != null)
                filter = CombineFilters(filter, o => o.UserId == userId.Value);

            if (paymentStatuses != null && paymentStatuses.Count > 0)
                filter = CombineFilters(filter, o => paymentStatuses.Contains(o.PaymentStatus));

            if (fulfillmentStatuses != null && fulfillmentStatuses.Count > 0)
                filter = CombineFilters(filter, o => fulfillmentStatuses.Contains(o.FulfillmentStatus));

            if (startDate.HasValue)
                filter = CombineFilters(filter, o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                filter = CombineFilters(filter, o => o.OrderDate <= endOfDay);
            }

            var result = await _orderRepository.GetAllAsyncWithIncludes(
                sortedPaging,
                filter,
                "OrderDetails.Product.Category",
                "Customer");

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load paged orders", ex);
        }
    }

    /// <summary>
    /// Lấy danh sách đơn hàng Active (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<Order>> GetPagedActiveOrders(PagingRequest paging, string? statusFilter = null)
    {
        System.Diagnostics.Debug.WriteLine($"📡 Service: GetPagedActiveOrders, filter={statusFilter}");
        if (paging == null) throw new ArgumentNullException(nameof(paging));
        paging.Validate();
        var sortedPaging = new PagingRequest
        {
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize,
            SearchText = paging.SearchText,
            Filters = paging.Filters,
            SortBy = "OrderDate DESC"
        };

        try
        {
            // Active logic: NOT Cancelled, NOT Refunded, AND NOT Completed
            Expression<Func<Order, bool>> filter = o =>
                o.PaymentStatus != "Cancelled" &&
                o.PaymentStatus != "Refunded" &&
                o.FulfillmentStatus != "Completed";

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All Active")
            {
                filter = CombineFilters(filter, o => o.FulfillmentStatus == statusFilter);
            }

            var result = await _orderRepository.GetAllAsyncWithIncludes(
                sortedPaging,
                filter,
                "OrderDetails.Product.Category",
                "Customer");

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load orders active", ex);
        }
    }

    /// <summary>
    /// Đếm số lượng đơn hàng Active (Bất đồng bộ)
    /// </summary>
    public async Task<int> GetActiveCount()
    {
        return await _orderRepository.CountAsync(o =>
            o.PaymentStatus != "Cancelled" &&
            o.PaymentStatus != "Refunded" &&
            o.FulfillmentStatus != "Completed");
    }

    /// <summary>
    /// Lấy danh sách đơn hàng History (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<Order>> GetPagedHistoryOrders(PagingRequest paging, DateTime? startDate = null, DateTime? endDate = null, string? statusFilter = null)
    {
        System.Diagnostics.Debug.WriteLine($"📡 Service: GetPagedHistoryOrders, filter={statusFilter}");
        if (paging == null) throw new ArgumentNullException(nameof(paging));
        paging.Validate();
        var sortedPaging = new PagingRequest
        {
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize,
            SearchText = paging.SearchText,
            Filters = paging.Filters,
            SortBy = "OrderDate DESC"
        };

        try
        {
            // History logic: (Paid AND Completed) OR Cancelled OR Refunded
            Expression<Func<Order, bool>> filter;

            if (string.IsNullOrEmpty(statusFilter) || statusFilter == "All History")
            {
                filter = o =>
                    (o.PaymentStatus == "Paid" && o.FulfillmentStatus == "Completed") ||
                    o.PaymentStatus == "Cancelled" ||
                    o.PaymentStatus == "Refunded";
            }
            else if (statusFilter == "Paid")
            {
                filter = o => o.PaymentStatus == "Paid" && o.FulfillmentStatus == "Completed";
            }
            else
            {
                filter = o => o.PaymentStatus == statusFilter;
            }

            if (startDate.HasValue)
            {
                // Chuyển 00:00:00 giờ Việt Nam sang giờ UTC để Database hiểu đúng
                var startUtc = startDate.Value.Date.ToUniversalTime();
                filter = CombineFilters(filter, o => o.OrderDate >= startUtc);
            }

            if (endDate.HasValue)
            {
                // Chuyển 23:59:59 giờ Việt Nam sang giờ UTC
                var endUtc = endDate.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
                filter = CombineFilters(filter, o => o.OrderDate <= endUtc);
            }

            var result = await _orderRepository.GetAllAsyncWithIncludes(
                sortedPaging,
                filter,
                "OrderDetails.Product",
                "Customer");

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load order history", ex);
        }
    }

    /// <summary>
    /// Đếm số lượng đơn hàng History (Bất đồng bộ)
    /// </summary>
    public async Task<int> GetHistoryCount(DateTime? startDate = null, DateTime? endDate = null)
    {
        Expression<Func<Order, bool>> filter = o =>
            (o.PaymentStatus == "Paid" && o.FulfillmentStatus == "Completed") ||
            o.PaymentStatus == "Cancelled" ||
            o.PaymentStatus == "Refunded";

        if (startDate.HasValue)
        {
            var startDateValue = startDate.Value;
            filter = CombineFilters(filter, o => o.OrderDate >= startDateValue);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);
            filter = CombineFilters(filter, o => o.OrderDate <= endOfDay);
        }

        return await _orderRepository.CountAsync(filter);
    }

    private static Expression<Func<Order, bool>> CombineFilters(
        Expression<Func<Order, bool>> expr1,
        Expression<Func<Order, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(Order), "o");

        var leftVisitor = new ParameterReplaceVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ParameterReplaceVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        return Expression.Lambda<Func<Order, bool>>(
            Expression.AndAlso(left, right),
            parameter);
    }

    private class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : base.VisitParameter(node);
        }
    }
    //Lấy đơn theo ngày
    public async Task<IEnumerable<Order>> GetOrdersByDateRange(DateTime from, DateTime to)    
    {        
        return await _orderRepository.ExecuteAsync(async context =>
        {
            return await context.Set<Order>()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= from && o.OrderDate < to)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }, "GetOrdersByDateRange");
    }

    /// <summary>
    /// Lấy tất cả đơn hàng kèm chi tiết (để tính best-seller)
    /// </summary>
    public async Task<IEnumerable<Order>> GetAllOrdersWithDetails()
    {
        return await _orderRepository.ExecuteAsync(async context =>
        {
            return await context.Set<Order>()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.PaymentStatus == "Paid")
                .ToListAsync();
        }, "GetAllOrdersWithDetails");
    }
    public async Task<int> GetTodayOrderCount()
    {
        // Lấy biên giới ngày hôm nay theo giờ máy chủ (Local) rồi đổi sang UTC
        var today = DateTime.Today;
        var startUtc = today.ToUniversalTime();
        var endUtc = today.AddDays(1).AddSeconds(-1).ToUniversalTime();

        return await _orderRepository.CountAsync(o => 
            o.OrderDate >= startUtc && 
            o.OrderDate <= endUtc && 
            o.PaymentStatus == "Paid");
    }

    public async Task<decimal> GetTodayRevenue()
    {
        var today = DateTime.Today;
        var startUtc = today.ToUniversalTime();
        var endUtc = today.AddDays(1).AddSeconds(-1).ToUniversalTime();

        return await _orderRepository.ExecuteAsync(async (AppDbContext context) =>
        {
            var result = await context.Set<Order>()
                .Where(o => o.OrderDate >= startUtc && o.OrderDate <= endUtc && o.PaymentStatus == "Paid")
                .SumAsync(o => (decimal?)o.TotalAmount);
            return result ?? 0m;
        }, "GetTodayRevenue");
    }

    public async Task<List<(string Label, decimal Value)>> GetRevenueByDateInMonth(int month, int year)
    {
        // Tạo mốc thời gian bắt đầu và kết thúc của tháng theo giờ Việt Nam
        // Sau đó đổi sang UTC để query Database
        var startLocal = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Local);
        var endLocal = startLocal.AddMonths(1).AddSeconds(-1);

        var startUtc = startLocal.ToUniversalTime();
        var endUtc = endLocal.ToUniversalTime();

        return await _orderRepository.ExecuteAsync(async (AppDbContext context) =>
        {
            // 1. Tải dữ liệu thô từ DB (chỉ lấy các trường cần thiết để tối ưu)
            var data = await context.Set<Order>()
                .AsNoTracking()
                .Where(o => o.OrderDate >= startUtc && o.OrderDate <= endUtc && o.PaymentStatus == "Paid")
                .Select(o => new { o.OrderDate, o.TotalAmount })
                .ToListAsync();

            // 2. Xử lý GroupBy ở Client-side để đảm bảo tính đúng theo giờ Local Việt Nam
            var rawData = data
                .GroupBy(o => o.OrderDate.ToLocalTime().Day)
                .Select(g => new { Day = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .OrderBy(x => x.Day)
                .ToList();

            return rawData.Select(x => (x.Day.ToString(), x.Total)).ToList();
        }, "GetRevenueByDateInMonth");
    }

public async Task<List<(string Name, int Quantity)>> GetTopSellingProducts(int limit = 5)
{
    return await _orderRepository.ExecuteAsync(async (AppDbContext context) =>
    {
        var rawData = await context.Set<OrderDetail>()
            .Include(od => od.Product)
            .GroupBy(od => od.Product!.Name)
            .Select(g => new { Name = g.Key, Total = g.Sum(od => od.Quantity) })
            .OrderByDescending(x => x.Total)
            .Take(limit)
            .ToListAsync();

        return rawData.Select(x => (x.Name, x.Total)).ToList();
    }, "GetTopSelling");
}
    public async Task<IEnumerable<Order>> GetRecentOrders(int limit = 3)
    {
        return await _orderRepository.ExecuteAsync(async (AppDbContext context) =>
        {
            var orders = await context.Set<Order>()
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate) // Sắp xếp theo thời gian mới nhất
                .Take(limit)
                .ToListAsync();

            // In log để debug kiểm tra
            foreach(var o in orders) {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Recent Order: ID={o.Id}, Date={o.OrderDate}, Total={o.TotalAmount}");
            }

            return orders;
        }, "GetRecentOrders");
    }
}

