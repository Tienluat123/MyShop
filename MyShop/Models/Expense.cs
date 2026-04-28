using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Models;

public partial class Expense : ObservableObject
{
    [ObservableProperty] private int id;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private string category = "Other"; // Rent, Utilities, Batch, Marketing, etc.
    
    // UI Helper
    [ObservableProperty] private bool isEditing;
}
