using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyShop.Models;
[Table("Customers")]
public partial class Customer : ObservableObject
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty; // SĐT dùng tìm kiếm
    public string? Address { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Notes { get; set; }

    // Loyalty properties
    public int Points { get; set; } = 0;
    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Regular;
    public string Status { get; set; } = "Active"; // Active/Inactive
    public int TotalVisits { get; set; } = 0;
    public double TotalSpent { get; set; } = 0;
    public DateTime? LastVisitDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

// Loyalty tier enum - used here and in PosModels
public enum LoyaltyTier
{
    Regular = 0,
    Silver = 1,
    Gold = 2,
    VIP = 3
}