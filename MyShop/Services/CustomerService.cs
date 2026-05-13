using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Services;

public interface ICustomerService
{
    Task<PagedResult<Customer>> GetAll(PagingRequest? paging = null);
    Task<Customer?> GetById(int id);
    Task<Customer?> GetByPhone(string phone);

    Task<List<Customer>> Search(string keyword);
    Task<Customer> Add(Customer customer);
    Task Update(Customer customer);
    Task UpdateStatus(int customerId, string status);

    Task Delete(int id);
}
public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repo;

    public CustomerService(IRepository<Customer> repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }
    public async Task<PagedResult<Customer>> GetAll(PagingRequest? paging = null)
    {
        try
        {
            // Nếu có SearchText, tạo filter expression để tìm kiếm
            System.Linq.Expressions.Expression<Func<Customer, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(paging?.SearchText))
            {
                var keyword = paging.SearchText.Trim();
                filter = c =>
                    EF.Functions.ToTsVector("simple", c.Name).Matches(EF.Functions.PlainToTsQuery("simple", keyword)) ||
                    EF.Functions.ILike(c.Name, $"%{keyword}%") ||
                    EF.Functions.ILike(c.Phone, $"%{keyword}%") ||
                    (c.Email != null && EF.Functions.ILike(c.Email, $"%{keyword}%")) ||
                    (c.Gender != null && EF.Functions.ILike(c.Gender, $"%{keyword}%"));
            }

            return await _repo.GetAllAsync(paging, filter);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load customers", ex);
        }
    }

    /// <summary>
    /// Lấy khách hàng theo ID (Bất đồng bộ)
    /// </summary>
    public async Task<Customer?> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be greater than 0", nameof(id));

        try
        {
            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load customer with ID {id}", ex);
        }
    }

    /// <summary>
    /// Tìm khách hàng theo số điện thoại (Bất đồng bộ)
    ///  BƯỚC 2: Filter server-side thay vì client-side
    /// </summary>
    public async Task<Customer?> GetByPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number is required", nameof(phone));

        try
        {
            // Sử dụng GetAsync với filter thay vì GetAllAsync mà không filter
            var customers = await _repo.GetAsync(c => c.Phone == phone);
            return customers.FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to find customer by phone number", ex);
        }
    }

    /// <summary>
    /// Tìm kiếm khách hàng (Bất đồng bộ)
    /// BƯỚC 2: Filter server-side thay vì client-side
    /// </summary>
    public async Task<List<Customer>> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<Customer>();

        try
        {
            var keywordTrimmed = keyword.Trim();
            // Sử dụng GetAsync với filter để lọc server-side bằng ILike
            var customers = await _repo.GetAsync(c =>
                EF.Functions.ToTsVector("simple", c.Name).Matches(EF.Functions.PlainToTsQuery("simple", keywordTrimmed)) ||
                EF.Functions.ILike(c.Name, $"%{keywordTrimmed}%") ||
                EF.Functions.ILike(c.Phone, $"%{keywordTrimmed}%") ||
                (c.Email != null && EF.Functions.ILike(c.Email, $"%{keywordTrimmed}%"))
            );
            return customers;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to search customers", ex);
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
            throw new ArgumentException("Customer name is required", nameof(customer.Name));

        if (string.IsNullOrWhiteSpace(customer.Phone))
            throw new ArgumentException("Phone number is required", nameof(customer.Phone));

        // Kiểm tra số điện thoại đã tồn tại
        var existingCustomer = await GetByPhone(customer.Phone);
        if (existingCustomer != null)
            throw new InvalidOperationException($"Customer with phone number {customer.Phone} already exists");

        try
        {
            return await _repo.AddAsync(customer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to add customer", ex);
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
            throw new ArgumentException("Invalid customer ID", nameof(customer.Id));

        if (string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Customer name is required", nameof(customer.Name));

        if (string.IsNullOrWhiteSpace(customer.Phone))
            throw new ArgumentException("Phone number is required", nameof(customer.Phone));

        try
        {
            await _repo.UpdateAsync(customer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update customer", ex);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái khách hàng (Active/Inactive)
    /// </summary>
    public async Task UpdateStatus(int customerId, string status)
    {
        if (customerId <= 0)
            throw new ArgumentException("Invalid customer ID", nameof(customerId));

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Invalid status", nameof(status));

        var normalizedStatus = status.Trim();
        if (!normalizedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
            !normalizedStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Status must be Active or Inactive", nameof(status));
        }

        normalizedStatus = normalizedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase)
            ? "Active"
            : "Inactive";

        try
        {
            var customer = await _repo.GetByIdAsync(customerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer not found with ID {customerId}");

            customer.Status = normalizedStatus;
            await _repo.UpdateAsync(customer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update customer status for ID {customerId}", ex);
        }
    }

    /// <summary>
    /// Xóa khách hàng (Bất đồng bộ)
    /// </summary>
    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid customer ID", nameof(id));

        try
        {
            await _repo.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete customer with ID {id}", ex);
        }
    }
}