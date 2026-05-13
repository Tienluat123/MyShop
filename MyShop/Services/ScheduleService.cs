using MyShop.Models;
using MyShop.Data;

namespace MyShop.Services;

public interface IScheduleService
{
    Task<List<WorkSchedule>> GetRostersForWeek(DateOnly weekStart);
    Task<bool> CreateAsync(WorkSchedule schedule);
    Task<bool> UpdateAsync(WorkSchedule schedule);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsUserAlreadyScheduledForShiftAsync(int userId, DateOnly date, string shiftName);

    event Action? OnScheduleChanged;
}

public class ScheduleService : IScheduleService
{
    private readonly IRepository<WorkSchedule> _repo;
    public event Action? OnScheduleChanged;

    public ScheduleService(IRepository<WorkSchedule> repo) => _repo = repo;

    private void Notify() => OnScheduleChanged?.Invoke();

    public async Task<List<WorkSchedule>> GetRostersForWeek(DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(7);
        return await _repo.GetAsync(
            s => s.WorkDate >= weekStart && s.WorkDate < weekEnd,
            nameof(WorkSchedule.User)
        );
    }

    public async Task<bool> IsUserAlreadyScheduledForShiftAsync(int userId, DateOnly date, string shiftName)
        => (await _repo.GetAsync(s =>
            s.UserId == userId &&
            s.WorkDate == date &&
            s.ShiftName == shiftName)).Any();

    private async Task<bool> ExecuteWithNotifyAsync(Func<Task> action, string actionName)
    {
        try
        {
            await action();
            Notify();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error {actionName}: {ex.Message}");
            return false;
        }
    }

    public Task<bool> CreateAsync(WorkSchedule schedule)
        => ExecuteWithNotifyAsync(() => _repo.AddAsync(schedule), "creating");

    public Task<bool> UpdateAsync(WorkSchedule schedule)
        => ExecuteWithNotifyAsync(() => _repo.UpdateAsync(schedule), "updating");

    public Task<bool> DeleteAsync(int id)
        => ExecuteWithNotifyAsync(() => _repo.DeleteAsync(id), "deleting");
}
