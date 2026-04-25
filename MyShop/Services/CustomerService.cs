using MyShop.Data;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Interface cho Customer Service
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Lấy tất cả khách hàng
    /// </summary>
    PagedResult<Customer> GetAll(PagingRequest? paging = null);

    /// <summary>
    /// Lấy khách hàng theo ID
    /// </summary>
    Customer? GetById(int id);

    /// <summary>
    /// Tìm khách hàng theo số điện thoại
    /// </summary>
    Customer? GetByPhone(string phone);

    /// <summary>
    /// Tìm kiếm khách hàng
    /// </summary>
    List<Customer> Search(string keyword);

    /// <summary>
    /// Thêm khách hàng mới
    /// </summary>
    Customer Add(Customer customer);

    /// <summary>
    /// Cập nhật khách hàng
    /// </summary>
    void Update(Customer customer);

    /// <summary>
    /// Xóa khách hàng
    /// </summary>
    void Delete(int id);
}

/// <summary>
/// Customer Service Implementation
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomerService(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    /// <summary>
    /// Lấy tất cả khách hàng
    /// </summary>
    public PagedResult<Customer> GetAll(PagingRequest? paging = null)
    {
        try
        {
            return _customerRepository.GetAll(paging);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách khách hàng", ex);
        }
    }

    /// <summary>
    /// Lấy khách hàng theo ID
    /// </summary>
    public Customer? GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            return _customerRepository.GetById(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy khách hàng với ID {id}", ex);
        }
    }

    /// <summary>
    /// Tìm khách hàng theo số điện thoại
    /// </summary>
    public Customer? GetByPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Số điện thoại không được để trống", nameof(phone));

        try
        {
            var allCustomers = _customerRepository.GetAll().Items;
            return allCustomers.FirstOrDefault(c => c.Phone == phone);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tìm khách hàng theo số điện thoại", ex);
        }
    }

    /// <summary>
    /// Tìm kiếm khách hàng
    /// </summary>
    public List<Customer> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<Customer>();

        try
        {
            var allCustomers = _customerRepository.GetAll().Items;
            return allCustomers
                .Where(c =>
                    c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.Phone.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email != null && c.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                )
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tìm kiếm khách hàng", ex);
        }
    }

    /// <summary>
    /// Thêm khách hàng mới
    /// </summary>
    public Customer Add(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        // ✅ Business logic validation
        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Tên khách hàng không được để trống", nameof(customer.Name));

        if (string.IsNullOrWhiteSpace(customer.Phone))
            throw new ArgumentException("Số điện thoại không được để trống", nameof(customer.Phone));

        // Kiểm tra số điện thoại đã tồn tại
        var existingCustomer = GetByPhone(customer.Phone);
        if (existingCustomer != null)
            throw new InvalidOperationException($"Khách hàng với số điện thoại {customer.Phone} đã tồn tại");

        try
        {
            return _customerRepository.Add(customer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi thêm khách hàng", ex);
        }
    }

    /// <summary>
    /// Cập nhật khách hàng
    /// </summary>
    public void Update(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (customer.Id <= 0)
            throw new ArgumentException("ID khách hàng không hợp lệ", nameof(customer.Id));

        // ✅ Business logic validation
        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Tên khách hàng không được để trống", nameof(customer.Name));

        if (string.IsNullOrWhiteSpace(customer.Phone))
            throw new ArgumentException("Số điện thoại không được để trống", nameof(customer.Phone));

        try
        {
            _customerRepository.Update(customer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi cập nhật khách hàng", ex);
        }
    }

    /// <summary>
    /// Xóa khách hàng
    /// </summary>
    public void Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID khách hàng không hợp lệ", nameof(id));

        try
        {
            _customerRepository.Delete(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xóa khách hàng với ID {id}", ex);
        }
    }
}
