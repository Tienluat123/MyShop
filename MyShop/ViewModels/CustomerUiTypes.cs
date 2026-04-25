using System;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public sealed class CustomerListItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string Tier { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int TotalAccumulatedPoints { get; set; } // Điểm tổng
    public int CurrentAvailablePoints { get; set; } // Điểm có sẵn để đổi
    public int Visits { get; set; }

    // UI Helper
    public string BirthdayDisplay => Birthday.HasValue ? Birthday.Value.ToString("dd/MM/yyyy") : "-";
}


public sealed class TierThreshold
{
    public string TierName { get; set; } = string.Empty;
    public int Points { get; set; }
}

public sealed class LoyaltyRule
{
    public int EarningRate { get; set; }
    public int MoneyUnit { get; set; }
    public ObservableCollection<TierThreshold> Thresholds { get; set; } = new();
}

public sealed class OrderHistoryItem
{
    public string OrderId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Items { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public string DateText => Date.ToString("yyyy-MM-dd");
    public string AmountText => $"${Amount:F2}";
}

public sealed class PointLogItem
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int DeltaPoints { get; set; }

    public bool IsPositive => DeltaPoints >= 0;
    public string SignedPoints => DeltaPoints >= 0 ? $"+{DeltaPoints} Pts" : $"{DeltaPoints} Pts";
    public string DeltaForeground => IsPositive ? "#10B981" : "#DC2626";
    public string DeltaGlyph => IsPositive ? "&#xE72D;" : "&#xE72E;";
    public string DateText => Date.ToString("yyyy-MM-dd");
}

public sealed class CustomerDetail
{
    public CustomerListItem Customer { get; set; } = new();
    public string Email { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public ObservableCollection<OrderHistoryItem> Orders { get; set; } = new();
    public ObservableCollection<PointLogItem> PointLogs { get; set; } = new();
    public string Notes { get; set; } = string.Empty;

    public string BirthdayText => Birthday.ToString("yyyy-MM-dd");
    public string TotalSpentText => $"${TotalSpent:F2}";
}

public sealed class CustomerInputModel
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Gender { get; set; } = "Male";  // ✅ Add Gender
    public DateTimeOffset? Birthday { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
