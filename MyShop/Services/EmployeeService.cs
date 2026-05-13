using MyShop.Data;
using MyShop.Models;
using MyShop.ViewModels.StaffManagement;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace MyShop.Services;

public interface IEmployeeService
{
    Task<EmployeeSalaryPageResult> GetPagedEmployeeSalariesAsync(PagingRequest request);

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

    public async Task<EmployeeSalaryPageResult> GetPagedEmployeeSalariesAsync(PagingRequest? paging = null)
    {
        paging ??= new PagingRequest();
        paging.Validate();

        using var context = _contextFactory.CreateDbContext();
        var now = DateTime.Now;
        var lastMonth = now.AddMonths(-1);

        Func<WorkSchedule, decimal> getShiftHours = s => s.ShiftName == "Morning" ? 6.0m
                                                      : s.ShiftName == "Afternoon" ? 6.0m
                                                      : s.ShiftName == "Evening" ? 6.0m
                                                      : 0m;

        var maxCurrentHours = await context.WorkSchedules
            .Where(s => s.WorkDate.Month == now.Month && s.WorkDate.Year == now.Year && s.Status == "Completed")
            .GroupBy(_ => 1)
            .Select(g => g.Sum(s => s.ShiftName == "Morning" ? 6.0m
                                   : s.ShiftName == "Afternoon" ? 6.0m
                                   : s.ShiftName == "Evening" ? 6.0m
                                   : 0m))
            .FirstOrDefaultAsync();

        var baseQuery = context.Users
            .Select(u => new
            {
                User = u,
                CurrentHours = u.WorkSchedules
                    .Where(s => s.WorkDate.Month == now.Month && s.WorkDate.Year == now.Year && s.Status == "Completed")
                    .Sum(s => s.ShiftName == "Morning" ? 6.0m
                            : s.ShiftName == "Afternoon" ? 6.0m
                            : s.ShiftName == "Evening" ? 6.0m
                            : 0m),
                LastMonthHours = u.WorkSchedules
                    .Where(s => s.WorkDate.Month == lastMonth.Month && s.WorkDate.Year == lastMonth.Year && s.Status == "Completed")
                    .Sum(s => s.ShiftName == "Morning" ? 6.0m
                            : s.ShiftName == "Afternoon" ? 6.0m
                            : s.ShiftName == "Evening" ? 6.0m
                            : 0m)
            });

        if (!string.IsNullOrEmpty(paging.SearchText))
        {
            var searchStr = paging.SearchText.Trim();
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ToTsVector("simple", x.User.Username ?? "").Matches(EF.Functions.PlainToTsQuery("simple", searchStr)) ||
                EF.Functions.ILike(x.User.FirstName ?? "", $"%{searchStr}%") ||
                EF.Functions.ILike(x.User.LastName ?? "", $"%{searchStr}%") ||
                EF.Functions.ILike(x.User.Username ?? "", $"%{searchStr}%") ||
                EF.Functions.ILike(x.User.Phone ?? "", $"%{searchStr}%"));
        }

        if (paging.Filters?.ContainsKey("Role") == true && !string.IsNullOrEmpty(paging.Filters["Role"]))
        {
            var roleFilter = paging.Filters["Role"];
            if (roleFilter == "Admin")
                baseQuery = baseQuery.Where(x => x.User.Role == "1");
            else if (roleFilter == "Sale")
                baseQuery = baseQuery.Where(x => x.User.Role == "2");
        }

        if (paging.Filters?.ContainsKey("Status") == true && !string.IsNullOrEmpty(paging.Filters["Status"]))
        {
            var statusFilter = paging.Filters["Status"];
            if (statusFilter == "Active")
                baseQuery = baseQuery.Where(x => x.User.IsActive);
            else if (statusFilter == "Inactive")
                baseQuery = baseQuery.Where(x => !x.User.IsActive);
        }

        var totalCount = await baseQuery.CountAsync();

        var sortedQuery = !string.IsNullOrWhiteSpace(paging.SortBy)
            ? baseQuery.OrderBy(paging.SortBy)
            : baseQuery.OrderByDescending(x => x.CurrentHours);

        var allFilteredData = await baseQuery.ToListAsync();
        var allFilteredSalaries = allFilteredData
            .Select(x => BuildSalaryInfo(x.User, x.CurrentHours, x.LastMonthHours, maxCurrentHours))
            .ToList();

        var pagedData = await sortedQuery
            .Skip(paging.GetSkip())
            .Take(paging.PageSize)
            .ToListAsync();

        var result = pagedData
            .Select(x => BuildSalaryInfo(x.User, x.CurrentHours, x.LastMonthHours, maxCurrentHours))
            .ToList();

        var topPerformer = allFilteredSalaries
            .OrderByDescending(x => x.TotalHours)
            .FirstOrDefault();

        return new EmployeeSalaryPageResult
        {
            Items = result,
            Pagination = new PagingMetadata
            {
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalItems = totalCount
            },
            TotalPayroll = allFilteredSalaries.Sum(x => x.FinalSalary),
            ActiveStaffCount = allFilteredSalaries.Count(x => x.User.IsActive),
            TopPerformerName = topPerformer != null
                ? $"{topPerformer.User.FullName} ({topPerformer.TotalHours}h)"
                : "None"
        };
    }

    private static UserSalaryInfo BuildSalaryInfo(User user, decimal currentHours, decimal lastMonthHours, decimal maxCurrentHours)
    {
        decimal bonusRate = 1.0m;
        if (currentHours == maxCurrentHours && currentHours > 100m) bonusRate = 1.15m;
        else if (currentHours > 100m) bonusRate = 1.1m;

        return new UserSalaryInfo
        {
            User = user,
            TotalHours = (double)currentHours,
            FinalSalary = user.HourlyRate * currentHours * bonusRate,
            LastMonthSalary = user.HourlyRate * lastMonthHours,
            BonusTag = bonusRate == 1.15m ? "TOP 1 (+15%)" : (bonusRate == 1.1m ? "+10%" : "0%"),
            TagColor = bonusRate == 1.15m ? new SolidColorBrush(Colors.Gold) :
                       (bonusRate == 1.1m ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Gray))
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
        var employee = await _repo.GetByIdAsync(userId);
        if (employee == null)
        {
            return;
        }

        employee.IsActive = false;
        await _repo.UpdateAsync(employee);
    }
}
