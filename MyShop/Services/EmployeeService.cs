using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Services;

public interface IEmployeeService
{
    Task<List<User>> GetActiveEmployeesAsync();
    Task<List<User>> GetEmployeesWithSalaryStatsAsync(int month, int year);

    Task AddEmployeeAsync(User employee);
    Task UpdateEmployeeAsync(User employee);
    Task DeleteEmployeeAsync(int userId);

    User? GetById(int id);
}
public class EmployeeService : IEmployeeService
{
    private readonly IRepository<User> _repo;
    private readonly AppDbContextFactory _contextFactory;

    public EmployeeService(IRepository<User> repo, AppDbContextFactory contextFactory)
    {
        _repo = repo;
        _contextFactory = contextFactory;
    }

    public async Task<List<User>> GetActiveEmployeesAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<List<User>> GetEmployeesWithSalaryStatsAsync(int month, int year)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users
            .Include(u => u.WorkSchedules)
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public User? GetById(int id)
    {
        return _repo.GetAll().Items?.FirstOrDefault(u => u.Id == id);
    }

    public async Task AddEmployeeAsync(User employee)
    {
        await _repo.AddAsync(employee);
    }

    public async Task UpdateEmployeeAsync(User employee)
    {
        await _repo.UpdateAsync(employee);
    }

    public async Task DeleteEmployeeAsync(int userId)
    {
        await _repo.DeleteAsync(userId);
    }
}