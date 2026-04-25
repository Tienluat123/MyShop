namespace MyShop.Models;

public class WorkSchedule
{
    public int Id { get; set; }
    public int UserId { get; set; } // Ai làm?
    public DateOnly WorkDate { get; set; } // Làm ngày nào? (VD: 2026-03-05)
    public TimeOnly StartTime { get; set; } // Giờ bắt đầu (VD: 08:00)
    public TimeOnly EndTime { get; set; } // Giờ kết thúc (VD: 12:00)
    public string ShiftName { get; set; } = string.Empty; // Tên ca (Ca Sáng, Ca Chiều)
    
    // Navigation properties
    public User? User { get; set; }
}
