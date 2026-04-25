using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Models;

/// <summary>
/// Customer entity — loyalty fields split per spec:
/// - TotalAccumulatedPoints: all-time points used for tier ranking
/// - CurrentAvailablePoints: spendable points for redemption
/// </summary>
public partial class Customer : ObservableObject
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Notes { get; set; }

    /// <summary>All-time accumulated points — used to determine Tier.</summary>
    [Column("TotalAccumulatedPoints")]
    [ObservableProperty]
    private int _totalAccumulatedPoints;

    /// <summary>Points currently available for redemption.</summary>
    [Column("CurrentAvailablePoints")]
    [ObservableProperty]
    private int _currentAvailablePoints;

    /// <summary>Membership tier derived from TotalAccumulatedPoints.</summary>
    [ObservableProperty]
    private LoyaltyTier _tier = LoyaltyTier.Bronze;

    public string Status { get; set; } = "Active"; // Active / Inactive

    [Column("TotalVisits")]
    [ObservableProperty]
    private int _totalVisits;

    [Column("TotalSpent")]
    [ObservableProperty]
    private double _totalSpent;

    public DateTime? LastVisitDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

/// <summary>
/// Loyalty membership tiers per Requirement.md spec:
/// Bronze (1x), Silver (1.5x), Gold (2x), Platinum (2.5x)
/// </summary>
public enum LoyaltyTier
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}
