using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface cho Order Service
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Lấy tất cả đơn hàng
    /// </summary>
    PagedResult<Order> GetAll(PagingRequest? paging = null);

    /// <summary>
    /// Lấy đơn hàng theo ID
    /// </summary>
    Order? GetById(int id);

    /// <summary>
    /// Lấy đơn hàng của khách hàng
    /// </summary>
    List<Order> GetByCustomerId(int customerId);

    /// <summary>
    /// Lấy đơn hàng theo trạng thái
    /// </summary>
    List<Order> GetByStatus(string status);

    /// <summary>
    /// Lấy đơn hàng chưa thanh toán
    /// </summary>
    List<Order> GetUnpaidOrders();

    /// <summary>
    /// Tính doanh thu trong khoảng ngày
    /// </summary>
    decimal GetRevenue(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Thêm đơn hàng mới
    /// </summary>
    Order Add(Order order);

    /// <summary>
    /// Cập nhật đơn hàng
    /// </summary>
    void Update(Order order);

    /// <summary>
    /// Xóa đơn hàng
    /// </summary>
    void Delete(int id);

    /// <summary>
    /// Cập nhật trạng thái đơn hàng
    /// </summary>
    void UpdateOrderStatus(int orderId, string newStatus);
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
    /// Lấy tất cả đơn hàng
    /// </summary>
    public PagedResult<Order> GetAll(PagingRequest? paging = null)
    {
        try
        {
            return _orderRepository.GetAll(paging);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách đơn hàng", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng theo ID
    /// </summary>
    public Order? GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            return _orderRepository.GetById(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy đơn hàng với ID {id}", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng của khách hàng
    /// </summary>
    public List<Order> GetByCustomerId(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID phải lớn hơn 0", nameof(customerId));

        try
        {
            var allOrders = _orderRepository.GetAll().Items;
            return allOrders
                .Where(o => o.CustomerId == customerId)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy đơn hàng của khách hàng {customerId}", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng theo trạng thái
    /// </summary>
    public List<Order> GetByStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Trạng thái không được để trống", nameof(status));

        try
        {
            var allOrders = _orderRepository.GetAll().Items;
            return allOrders
                .Where(o => o.Status == status)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy đơn hàng theo trạng thái '{status}'", ex);
        }
    }

    /// <summary>
    /// Lấy đơn hàng chưa thanh toán
    /// </summary>
    public List<Order> GetUnpaidOrders()
    {
        try
        {
            return GetByStatus("New");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy đơn hàng chưa thanh toán", ex);
        }
    }

    /// <summary>
    /// Tính doanh thu trong khoảng ngày
    /// </summary>
    public decimal GetRevenue(DateTime startDate, DateTime endDate)
    {
        try
        {
            var allOrders = _orderRepository.GetAll().Items;
            return allOrders
                .Where(o =>
                    o.OrderDate >= startDate &&
                    o.OrderDate <= endDate &&
                    o.Status == "Paid"
                )
                .Sum(o => o.TotalAmount);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi tính doanh thu", ex);
        }
    }

    /// <summary>
    /// Thêm đơn hàng mới
    /// </summary>
    public Order Add(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        // ✅ Business logic validation
        if (order.CustomerId <= 0)
            throw new ArgumentException("Phải chọn khách hàng hợp lệ", nameof(order.CustomerId));

        if (order.UserId <= 0)
            throw new ArgumentException("Phải chọn nhân viên hợp lệ", nameof(order.UserId));

        if (order.TotalAmount < 0)
            throw new ArgumentException("Tổng tiền không được âm", nameof(order.TotalAmount));

        // Kiểm tra customer tồn tại
        var customer = _customerRepository.GetById(order.CustomerId);
        if (customer == null)
            throw new InvalidOperationException($"Khách hàng với ID {order.CustomerId} không tồn tại");

        // Kiểm tra user tồn tại
        var user = _userRepository.GetById(order.UserId);
        if (user == null)
            throw new InvalidOperationException($"Nhân viên với ID {order.UserId} không tồn tại");

        try
        {
            return _orderRepository.Add(order);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi thêm đơn hàng", ex);
        }
    }

    /// <summary>
    /// Cập nhật đơn hàng
    /// </summary>
    public void Update(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (order.Id <= 0)
            throw new ArgumentException("ID đơn hàng không hợp lệ", nameof(order.Id));

        try
        {
            _orderRepository.Update(order);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi cập nhật đơn hàng", ex);
        }
    }

    /// <summary>
    /// Xóa đơn hàng
    /// </summary>
    public void Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID đơn hàng không hợp lệ", nameof(id));

        try
        {
            _orderRepository.Delete(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xóa đơn hàng với ID {id}", ex);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái đơn hàng
    /// </summary>
    public void UpdateOrderStatus(int orderId, string newStatus)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order ID không hợp lệ", nameof(orderId));

        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Trạng thái mới không được để trống", nameof(newStatus));

        try
        {
            var order = GetById(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Đơn hàng với ID {orderId} không tìm thấy");

            order.Status = newStatus;
            Update(order);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi cập nhật trạng thái đơn hàng", ex);
        }
    }
}
