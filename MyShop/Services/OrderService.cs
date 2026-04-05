using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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
    Task<PagedResult<Order>> GetPagedOrdersByStatuses(PagingRequest paging, bool sortDescending, DateTime? startDate = null, DateTime? endDate = null, List<string>? paymentStatuses = null, List<string>? fulfillmentStatuses = null);

    /// <summary>
    /// Đếm số lượng đơn hàng theo danh sách trạng thái
    /// </summary>
    Task<int> GetCountByStatus(DateTime? startDate = null, DateTime? endDate = null, List<string>? paymentStatuses = null, List<string>? fulfillmentStatuses = null);

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
            return await _orderRepository.GetAll(paging);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách đơn hàng", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng theo ID (Bất đồng bộ)
    /// </summary>
    public async Task<Order?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            return await _orderRepository.GetById(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy đơn hàng với ID {id}", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng của khách hàng (Bất đồng bộ)
    /// </summary>
    public async Task<List<Order>> GetByCustomerId(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID phải lớn hơn 0", nameof(customerId));

        try
        {
            var items = await _orderRepository.Find(o => o.CustomerId == customerId);
            return items.ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy đơn hàng của khách hàng {customerId}", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng theo PaymentStatus (Bất đồng bộ)
    /// </summary>
    public async Task<List<Order>> GetByPaymentStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Trạng thái không được để trống", nameof(status));

        try
        {
            var items = await _orderRepository.Find(o => o.PaymentStatus == status);
            return items.ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy đơn hàng theo thanh toán '{status}'", ex);
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
            throw new InvalidOperationException("Lỗi khi lấy đơn hàng chưa thanh toán", ex);
        }
    }

    /// <summary>
    /// Tính doanh thu trong khoảng ngày (Bất đồng bộ)
    /// </summary>
    public async Task<decimal> GetRevenue(DateTime startDate, DateTime endDate)
    {
        try
        {
            var sum = await _orderRepository.Query()
                .Where(o =>
                    o.OrderDate >= startDate &&
                    o.OrderDate <= endDate &&
                    o.PaymentStatus == "Paid"
                )
                .SumAsync(o => (decimal?)o.TotalAmount);
            return sum ?? 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi tính doanh thu", ex);
        }
    }

    /// <summary>
    /// Thêm đơn hàng mới (Bất đồng bộ)
    /// </summary>
    public async Task<Order> Add(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.CustomerId <= 0)
            throw new ArgumentException("Phải chọn khách hàng hợp lệ", nameof(order.CustomerId));

        if (order.UserId <= 0)
            throw new ArgumentException("Phải chọn nhân viên hợp lệ", nameof(order.UserId));

        if (order.OrderDetails?.Any() == true)
        {
            order.SubTotal = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
            order.TotalAmount = order.SubTotal - order.DiscountAmount;
        }

        if (order.TotalAmount < 0)
            throw new ArgumentException("Tổng tiền không được âm", nameof(order.TotalAmount));

        var customer = await _customerRepository.GetById(order.CustomerId);
        if (customer == null)
            throw new InvalidOperationException($"Khách hàng với ID {order.CustomerId} không tồn tại");

        var user = await _userRepository.GetById(order.UserId);
        if (user == null)
            throw new InvalidOperationException($"Nhân viên với ID {order.UserId} không tồn tại");

        try
        {
            return await _orderRepository.Add(order);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi thêm đơn hàng", ex);
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
            throw new ArgumentException("ID đơn hàng không hợp lệ", nameof(order.Id));

        try
        {
            await _orderRepository.Update(order);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi cập nhật đơn hàng", ex);
        }
    }

    /// <summary>
    /// Xóa đơn hàng (Bất đồng bộ)
    /// </summary>
    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID đơn hàng không hợp lệ", nameof(id));

        try
        {
            await _orderRepository.Delete(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xóa đơn hàng với ID {id}", ex);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái thanh toán (Bất đồng bộ)
    /// </summary>
    public async Task UpdatePaymentStatus(int orderId, string newStatus)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order ID không hợp lệ", nameof(orderId));

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Trạng thái thanh toán mới không được để trống", nameof(newStatus));

        try
        {
            var order = await GetById(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tìm thấy");

            if (order.PaymentStatus == "Paid" && newStatus != "Paid" && newStatus != "Refunded")
                throw new InvalidOperationException("Không thể thay đổi trạng thái của đơn hàng đã thanh toán");
            
            if (order.PaymentStatus == "Cancelled")
                throw new InvalidOperationException("Không thể thay đổi trạng thái của đơn hàng đã hủy");

            order.PaymentStatus = newStatus;
            await Update(order);
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Lỗi hệ thống khi cập nhật trạng thái thanh toán", ex);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái fulfillment (Bất đồng bộ)
    /// </summary>
    public async Task UpdateFulfillmentStatus(int orderId, string newStatus)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order ID không hợp lệ", nameof(orderId));

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Trạng thái giao/nhận không được để trống", nameof(newStatus));

        try
        {
            var order = await GetById(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tìm thấy");

            order.FulfillmentStatus = newStatus;
            await Update(order);
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException($"Lỗi hệ thống khi cập nhật trạng thái fulfillment", ex);
        }
    }

    /// <summary>
    /// Đếm số lượng đơn hàng theo danh sách trạng thái (Bất đồng bộ)
    /// </summary>
    public async Task<int> GetCountByStatus(DateTime? startDate = null, DateTime? endDate = null, List<string>? paymentStatuses = null, List<string>? fulfillmentStatuses = null)
    {
        try
        {
            var query = _orderRepository.Query();
            
            if (paymentStatuses != null && paymentStatuses.Count > 0)
                query = query.Where(o => paymentStatuses.Contains(o.PaymentStatus));
            
            if (fulfillmentStatuses != null && fulfillmentStatuses.Count > 0)
                query = query.Where(o => fulfillmentStatuses.Contains(o.FulfillmentStatus));
            
            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endOfDay);
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi đếm số lượng đơn hàng", ex);
        }
    }

    /// <summary>
    /// Lấy danh sách đơn hàng được lọc và sắp xếp, hỗ trợ phân trang kép (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<Order>> GetPagedOrdersByStatuses(PagingRequest paging, bool sortDescending, DateTime? startDate = null, DateTime? endDate = null, List<string>? paymentStatuses = null, List<string>? fulfillmentStatuses = null)
    {
        if (paging == null) throw new ArgumentNullException(nameof(paging));
        paging.Validate();

        try
        {
            var query = _orderRepository.Query()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .AsQueryable();
            
            if (paymentStatuses != null && paymentStatuses.Count > 0)
                query = query.Where(o => paymentStatuses.Contains(o.PaymentStatus));
            
            if (fulfillmentStatuses != null && fulfillmentStatuses.Count > 0)
                query = query.Where(o => fulfillmentStatuses.Contains(o.FulfillmentStatus));

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endOfDay);
            }

            if (sortDescending)
                query = query.OrderByDescending(o => o.OrderDate);
            else
                query = query.OrderBy(o => o.OrderDate);

            var totalItems = await query.CountAsync();
            var items = await query.Skip(paging.GetSkip()).Take(paging.PageSize).ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                Pagination = new PagingMetadata
                {
                    PageNumber = paging.PageNumber,
                    PageSize = paging.PageSize,
                    TotalItems = totalItems
                }
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách đơn hàng phân trang", ex);
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

        try
        {
            IQueryable<Order> query = _orderRepository.Query()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer);

            // Active logic: NOT Cancelled, NOT Refunded, AND NOT Completed
            query = query.Where(o => o.PaymentStatus != "Cancelled" && o.PaymentStatus != "Refunded" && o.FulfillmentStatus != "Completed");

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All Active")
            {
                query = query.Where(o => o.FulfillmentStatus == statusFilter);
            }

            query = query.OrderByDescending(o => o.OrderDate);

            var totalItems = await query.CountAsync();
            var items = await query.Skip(paging.GetSkip()).Take(paging.PageSize).ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                Pagination = new PagingMetadata { PageNumber = paging.PageNumber, PageSize = paging.PageSize, TotalItems = totalItems }
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách đơn hàng active", ex);
        }
    }

    /// <summary>
    /// Đếm số lượng đơn hàng Active (Bất đồng bộ)
    /// </summary>
    public async Task<int> GetActiveCount()
    {
        return await _orderRepository.Query()
            .Where(o => o.PaymentStatus != "Cancelled" && o.PaymentStatus != "Refunded" && o.FulfillmentStatus != "Completed")
            .CountAsync();
    }

    /// <summary>
    /// Lấy danh sách đơn hàng History (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<Order>> GetPagedHistoryOrders(PagingRequest paging, DateTime? startDate = null, DateTime? endDate = null, string? statusFilter = null)
    {
        System.Diagnostics.Debug.WriteLine($"📡 Service: GetPagedHistoryOrders, filter={statusFilter}");
        if (paging == null) throw new ArgumentNullException(nameof(paging));
        paging.Validate();

        try
        {
            IQueryable<Order> query = _orderRepository.Query()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer);

            // History logic: (Paid AND Completed) OR Cancelled OR Refunded
            if (string.IsNullOrEmpty(statusFilter) || statusFilter == "All History")
            {
                query = query.Where(o => 
                    (o.PaymentStatus == "Paid" && o.FulfillmentStatus == "Completed") ||
                    o.PaymentStatus == "Cancelled" ||
                    o.PaymentStatus == "Refunded");
            }
            else if (statusFilter == "Paid")
            {
                query = query.Where(o => o.PaymentStatus == "Paid" && o.FulfillmentStatus == "Completed");
            }
            else
            {
                query = query.Where(o => o.PaymentStatus == statusFilter);
            }

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= endOfDay);
            }

            query = query.OrderByDescending(o => o.OrderDate);

            var totalItems = await query.CountAsync();
            var items = await query.Skip(paging.GetSkip()).Take(paging.PageSize).ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                Pagination = new PagingMetadata { PageNumber = paging.PageNumber, PageSize = paging.PageSize, TotalItems = totalItems }
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách đơn hàng lịch sử", ex);
        }
    }

    /// <summary>
    /// Đếm số lượng đơn hàng History (Bất đồng bộ)
    /// </summary>
    public async Task<int> GetHistoryCount(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _orderRepository.Query()
            .Where(o => 
                (o.PaymentStatus == "Paid" && o.FulfillmentStatus == "Completed") ||
                o.PaymentStatus == "Cancelled" ||
                o.PaymentStatus == "Refunded");

        if (startDate.HasValue)
            query = query.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(o => o.OrderDate <= endOfDay);
        }

        return await query.CountAsync();
    }
}
