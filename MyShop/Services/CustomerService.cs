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
    Task<PagedResult<Customer>> GetAll(PagingRequest? paging = null);

    /// <summary>
    /// Lấy khách hàng theo ID
    /// </summary>
    Task<Customer?> GetById(int id);

    /// <summary>
    /// Tìm khách hàng theo số điện thoại
    /// </summary>
    Task<Customer?> GetByPhone(string phone);

    /// <summary>
    /// Tìm kiếm khách hàng
    /// </summary>
    Task<List<Customer>> Search(string keyword);

    /// <summary>
    /// Thêm khách hàng mới
    /// </summary>
    Task<Customer> Add(Customer customer);

    /// <summary>
    /// Cập nhật khách hàng
    /// </summary>
    Task Update(Customer customer);

    /// <summary>
    /// Xóa khách hàng
    /// </summary>
    Task Delete(int id);
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
    /// Lấy tất cả khách hàng (Bất đồng bộ)
    /// </summary>
    public async Task<PagedResult<Customer>> GetAll(PagingRequest? paging = null)
    {
        try
        {
            // Nếu có SearchText, tạo filter expression để tìm kiếm
            System.Linq.Expressions.Expression<Func<Customer, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(paging?.SearchText))
            {
                var keyword = paging.SearchText.ToLower().Trim();
                filter = c =>
                    c.Name.ToLower().Contains(keyword) ||
                    c.Phone.Contains(keyword) ||
                    (c.Email != null && c.Email.ToLower().Contains(keyword)) ||
                    (c.Gender != null && c.Gender.ToLower().Contains(keyword));
            }

            return await _customerRepository.GetAllAsync(paging, filter);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi lấy danh sách khách hàng", ex);
        }
    }

    /// <summary>
    /// Lấy khách hàng theo ID (Bất đồng bộ)
    /// </summary>
    public async Task<Customer?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));

        try
        {
            return await _customerRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi lấy khách hàng với ID {id}", ex);
        }
    }

    /// <summary>
    /// Tìm khách hàng theo số điện thoại (Bất đồng bộ)
    ///  BƯỚC 2: Filter server-side thay vì client-side
    /// </summary>
    public async Task<Customer?> GetByPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Số điện thoại không được để trống", nameof(phone));

        try
        {
            // Sử dụng GetAsync với filter thay vì GetAllAsync mà không filter
            var customers = await _customerRepository.GetAsync(c => c.Phone == phone);
            return customers.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tìm khách hàng theo số điện thoại", ex);
        }
    }

    /// <summary>
    /// Tìm kiếm khách hàng (Bất đồng bộ)
    /// ✅ BƯỚC 2: Filter server-side thay vì client-side
    /// </summary>
    public async Task<List<Customer>> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<Customer>();

        try
        {
            var keywordLower = keyword.ToLower();
            // Sử dụng GetAsync với filter để lọc server-side
            var customers = await _customerRepository.GetAsync(c =>
                c.Name.ToLower().Contains(keywordLower) ||
                c.Phone.Contains(keyword) ||
                (c.Email != null && c.Email.ToLower().Contains(keywordLower))
            );
            return customers;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi tìm kiếm khách hàng", ex);
        }
    }

    /// <summary>
    /// Thêm khách hàng mới (Bất đồng bộ)
    /// </summary>
    public async Task<Customer> Add(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Tên khách hàng không được để trống", nameof(customer.Name));

        if (string.IsNullOrWhiteSpace(customer.Phone))
            throw new ArgumentException("Số điện thoại không được để trống", nameof(customer.Phone));

        // Kiểm tra số điện thoại đã tồn tại
        var existingCustomer = await GetByPhone(customer.Phone);
        if (existingCustomer != null)
            throw new InvalidOperationException($"Khách hàng với số điện thoại {customer.Phone} đã tồn tại");

        try
        {
            return await _customerRepository.AddAsync(customer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi thêm khách hàng", ex);
        }
    }

    /// <summary>
    /// Cập nhật khách hàng (Bất đồng bộ)
    /// </summary>
    public async Task Update(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (customer.Id <= 0)
            throw new ArgumentException("ID khách hàng không hợp lệ", nameof(customer.Id));

        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Tên khách hàng không được để trống", nameof(customer.Name));

        if (string.IsNullOrWhiteSpace(customer.Phone))
            throw new ArgumentException("Số điện thoại không được để trống", nameof(customer.Phone));

        try
        {
            await _customerRepository.UpdateAsync(customer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lỗi khi cập nhật khách hàng", ex);
        }
    }

    /// <summary>
    /// Xóa khách hàng (Bất đồng bộ)
    /// </summary>
    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID khách hàng không hợp lệ", nameof(id));

        try
        {
            await _customerRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi xóa khách hàng với ID {id}", ex);
        }
    }
}