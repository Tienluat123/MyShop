namespace MyShop.ViewModels;

public sealed class EmployeeListItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ShiftCount { get; set; }
    public string PositionColor { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;

    public string ScheduleButtonText => ShiftCount > 0 ? "Xem lịch" : "Phân ca";
}
