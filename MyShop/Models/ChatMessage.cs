using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Models;

public partial class ChatMessage : ObservableObject
{
    public string Content { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public bool IsProposal { get; set; }
    public string? ActionId { get; set; }
    
    public HorizontalAlignment HorizontalAlignment => IsUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    
    public Brush Background => IsUser 
        ? (Brush)Application.Current.Resources["BrandPrimary"]
        : (Brush)Application.Current.Resources["ControlFillColorDefaultBrush"];
}
