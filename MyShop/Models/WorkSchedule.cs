namespace MyShop.Models;

public class WorkSchedule
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly WorkDate { get; set; }
    public string ShiftName { get; set; } = string.Empty; // "Morning", "Afternoon", "Evening"
    public string? Note { get; set; }
    public string Status { get; set; } = "Scheduled"; // "Scheduled", "Completed", "Cancelled"

    public double TotalHours => ShiftName switch
    {
        "Morning" => 6.0, // 8h - 14h là 6 tiếng
        "Afternoon" => 6.0, // 14h - 20h là 6 tiếng
        "Evening" => 6.0, // 20h - 02h là 6 tiếng
        _ => 0
    };

    public User? User { get; set; }
}