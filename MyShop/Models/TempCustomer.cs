using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class TempCustomer : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _phone;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _gender;

    [ObservableProperty]
    private DateTime? _birthday;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private int _points;

    [ObservableProperty]
    private LoyaltyTier _tier = LoyaltyTier.Regular;

    [ObservableProperty]
    private string _status = "Active";

    [ObservableProperty]
    private int _totalVisits;

    [ObservableProperty]
    private double _totalSpent;

    [ObservableProperty]
    private DateTime? _lastVisitDate;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;
}
