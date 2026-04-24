using MyShop.Data;
using MyShop.Models;
using MyShop.ViewModels.StaffManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace MyShop.Services;

public interface IEmployeeService
{
    Task<PagedResult<UserSalaryInfo>> GetPagedEmployeeSalariesAsync(PagingRequest request);

    Task<List<User>> GetActiveEmployeesAsync();
    Task<User?> GetById(int id);
    Task AddEmployeeAsync(User employee);
    Task UpdateEmployeeAsync(User employee);
    Task DeleteEmployeeAsync(int userId);
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

    /// <summary>
    /// Get paginated employee salaries with all calculations done at DB level
    /// </summary>
    public async Task<PagedResult<UserSalaryInfo>> GetPagedEmployeeSalariesAsync(PagingRequest? paging = null)
    {
        paging?.Validate();

        using var context = _contextFactory.CreateDbContext();
        var now = DateTime.Now;
        var lastMonth = now.AddMonths(-1);

        // Helper expression to calculate hours from ShiftName (since TotalHours is a computed property, can't be translated to SQL)
        // Morning = 6h, Afternoon = 6h, Evening = 6h
        Func<WorkSchedule, double> getShiftHours = s => s.ShiftName == "Morning" ? 6.0
                                                      : s.ShiftName == "Afternoon" ? 6.0
                                                      : s.ShiftName == "Evening" ? 6.0
                                                      : 0;

        // Get MAX hours for current month (for bonus calculation)
        var maxCurrentHours = await context.WorkSchedules
            .Where(s => s.WorkDate.Month == now.Month && s.WorkDate.Year == now.Year && s.Status == "Completed")
            .GroupBy(_ => 1)
            .Select(g => g.Sum(s => s.ShiftName == "Morning" ? 6.0
                                   : s.ShiftName == "Afternoon" ? 6.0
                                   : s.ShiftName == "Evening" ? 6.0
                                   : 0))
            .FirstOrDefaultAsync();

        // Build base query with salary calculations
        var baseQuery = context.Users
            .Where(u => u.IsActive)
            .Select(u => new
            {
                User = u,
                // Current month total hours - calculate from ShiftName (not the computed property)
                CurrentHours = u.WorkSchedules
                    .Where(s => s.WorkDate.Month == now.Month && s.WorkDate.Year == now.Year && s.Status == "Completed")
                    .Sum(s => s.ShiftName == "Morning" ? 6.0
                            : s.ShiftName == "Afternoon" ? 6.0
                            : s.ShiftName == "Evening" ? 6.0
                            : 0),
                // Last month total hours for comparison
                LastMonthHours = u.WorkSchedules
                    .Where(s => s.WorkDate.Month == lastMonth.Month && s.WorkDate.Year == lastMonth.Year && s.Status == "Completed")
                    .Sum(s => s.ShiftName == "Morning" ? 6.0
                            : s.ShiftName == "Afternoon" ? 6.0
                            : s.ShiftName == "Evening" ? 6.0
                            : 0)
            });

        // Apply search filter
        if (!string.IsNullOrEmpty(paging?.SearchText))
        {
            var searchLower = paging.SearchText.ToLower();
            baseQuery = baseQuery.Where(x =>
                (x.User.FirstName ?? "").ToLower().Contains(searchLower) ||
                (x.User.LastName ?? "").ToLower().Contains(searchLower) ||
                (x.User.Username ?? "").ToLower().Contains(searchLower) ||
                (x.User.Phone ?? "").ToLower().Contains(searchLower));
        }

        // Apply role filter
        if (paging?.Filters?.ContainsKey("Role") == true && !string.IsNullOrEmpty(paging.Filters["Role"]))
        {
            var roleFilter = paging.Filters["Role"];
            if (roleFilter == "Admin")
                baseQuery = baseQuery.Where(x => x.User.Role == "1");
            else if (roleFilter == "Sale")
                baseQuery = baseQuery.Where(x => x.User.Role == "2");
        }

        // Apply status filter
        if (paging?.Filters?.ContainsKey("Status") == true && !string.IsNullOrEmpty(paging.Filters["Status"]))
        {
            var statusFilter = paging.Filters["Status"];
            if (statusFilter == "Active")
                baseQuery = baseQuery.Where(x => x.User.IsActive);
            else if (statusFilter == "Inactive")
                baseQuery = baseQuery.Where(x => !x.User.IsActive);
        }

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Apply sorting - comprehensive support for all columns
        var sortedQuery = baseQuery;
        if (!string.IsNullOrEmpty(paging?.SortColumn))
        {
            sortedQuery = paging.SortColumn switch
            {
                // Employee name column - sort by FirstName (FullName is computed and not in DB)
                "FullName" => paging.SortAscending
                    ? baseQuery.OrderBy(x => x.User.FirstName).ThenBy(x => x.User.LastName)
                    : baseQuery.OrderByDescending(x => x.User.FirstName).ThenByDescending(x => x.User.LastName),

                // Contact / Phone column
                "Phone" => paging.SortAscending
                    ? baseQuery.OrderBy(x => x.User.Phone)
                    : baseQuery.OrderByDescending(x => x.User.Phone),

                // Role column - sort by role code (1=Admin, 2=Sale)
                "Role" => paging.SortAscending
                    ? baseQuery.OrderBy(x => x.User.Role)
                    : baseQuery.OrderByDescending(x => x.User.Role),

                // Status column - sort by IsActive boolean
                "Status" => paging.SortAscending
                    ? baseQuery.OrderBy(x => x.User.IsActive)
                    : baseQuery.OrderByDescending(x => x.User.IsActive),

                // Hourly Rate column
                "HourlyRate" => paging.SortAscending
                    ? baseQuery.OrderBy(x => x.User.HourlyRate)
                    : baseQuery.OrderByDescending(x => x.User.HourlyRate),

                // Total Hours column - current month hours
                "TotalHours" => paging.SortAscending
                    ? baseQuery.OrderBy(x => x.CurrentHours)
                    : baseQuery.OrderByDescending(x => x.CurrentHours),

                // Last Month Salary column - sort by previous month calculation
                "LastMonthSalary" => paging.SortAscending
                    ? baseQuery.OrderBy(x => x.User.HourlyRate * (decimal)x.LastMonthHours)
                    : baseQuery.OrderByDescending(x => x.User.HourlyRate * (decimal)x.LastMonthHours),

                // Final Salary column - sort by current salary with bonus calculation
                "FinalSalary" => paging.SortAscending
                    ? baseQuery.OrderBy(x => x.User.HourlyRate * (decimal)x.CurrentHours)
                    : baseQuery.OrderByDescending(x => x.User.HourlyRate * (decimal)x.CurrentHours),

                // Default: sort by TotalHours descending
                _ => baseQuery.OrderByDescending(x => x.CurrentHours)
            };
        }
        else
        {
            sortedQuery = baseQuery.OrderByDescending(x => x.CurrentHours);
        }

        // Apply pagination at DB level
        var pagedData = await sortedQuery
            .Skip(paging.GetSkip())
            .Take(paging.PageSize)
            .ToListAsync();

        // Transform to UserSalaryInfo with bonus calculation
        var result = pagedData.Select(x =>
        {
            double currentHours = x.CurrentHours;
            double lastMonthHours = x.LastMonthHours;

            // Calculate bonus rate
            decimal bonusRate = 1.0m;
            if (currentHours == maxCurrentHours && currentHours > 100) bonusRate = 1.15m;
            else if (currentHours > 100) bonusRate = 1.1m;

            decimal currentSalary = x.User.HourlyRate * (decimal)currentHours * bonusRate;
            decimal lastMonthSalary = x.User.HourlyRate * (decimal)lastMonthHours;

            return new UserSalaryInfo
            {
                User = x.User,
                TotalHours = currentHours,
                FinalSalary = currentSalary,
                LastMonthSalary = lastMonthSalary,
                BonusTag = bonusRate == 1.15m ? "TOP 1 (+15%)" : (bonusRate == 1.1m ? "+10%" : "0%"),
                TagColor = bonusRate == 1.15m ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gold) :
                           (bonusRate == 1.1m ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGreen) : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray))
            };
        }).ToList();

        return new PagedResult<UserSalaryInfo>
        {
            Items = result,
            Pagination = new PagingMetadata
            {
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalItems = totalCount
            }
        };
    }

    public async Task<User?> GetById(int id)
    {
        return await _repo.GetByIdAsync(id);
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

