using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Models;
using MyShop.Services;
using MyShop.Di;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyShop.Views.Controls;

public sealed partial class ChatBotControl : UserControl
{
    private readonly IAiAssistantService _aiService;
    private readonly IPromotionService _promotionService;
    private MyShop.Models.Promotion? _pendingPromotion;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ChatBotControl()
    {
        this.InitializeComponent();
        _aiService = ServiceContainer.Instance.Get<IAiAssistantService>();
        _promotionService = ServiceContainer.Instance.Get<IPromotionService>();
        MessageList.ItemsSource = Messages;
        
        // Initial Greeting
        Messages.Add(new ChatMessage 
        { 
            Content = "Hello! I am your MyShop AI Assistant. I can help you check inventory, view reports, or create promo codes. How can I help you today?", 
            IsUser = false 
        });
    }

    private void ChatButton_Click(object sender, RoutedEventArgs e)
    {
        ChatPopup.IsOpen = !ChatPopup.IsOpen;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        ChatPopup.IsOpen = false;
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        SendMessage();
    }

    private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            SendMessage();
        }
    }

    private async void SendMessage()
    {
        var text = InputBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        Messages.Add(new ChatMessage { Content = text, IsUser = true });
        InputBox.Text = string.Empty;

        // Lấy chế độ hiện tại từ ComboBox
        var selectedItem = ModeSelector.SelectedItem as ComboBoxItem;
        var mode = selectedItem?.Content?.ToString() ?? "Q&A";

        // Show a loading indicator
        var response = await _aiService.GetAiResponse(text, Messages.ToList(), mode);
        
        // Parse for Proposals (Quét toàn bộ tin nhắn, không quan trọng có thẻ PROPOSAL: hay không)
        var codeMatch = System.Text.RegularExpressions.Regex.Match(response, @"CODE\s*[=:]\s*([^,\s\]\n]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var valueMatch = System.Text.RegularExpressions.Regex.Match(response, @"VALUE\s*[=:]\s*([^,\s\]%\n]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var typeMatch = System.Text.RegularExpressions.Regex.Match(response, @"TYPE\s*[=:]\s*([^,\]\n]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (codeMatch.Success && valueMatch.Success)
        {
            string promoCode = codeMatch.Groups[1].Value.Trim();
            string promoValue = valueMatch.Groups[1].Value.Trim();
            string promoType = typeMatch.Success ? typeMatch.Groups[1].Value.Trim() : "Percent";

            // Chặn lỗi AI lấy ID (số) làm mã
            if (System.Text.RegularExpressions.Regex.IsMatch(promoCode, @"^\d+$"))
            {
                promoCode = "KM-" + promoCode;
            }

            // CHUẨN HÓA MÃ: Thay thế dấu gạch ngang (-) thành dấu gạch dưới (_) để không bị lỗi DB
            promoCode = promoCode.Replace("-", "_");

            _pendingPromotion = new MyShop.Models.Promotion
            {
                PromoCode = promoCode,
                DiscountValue = decimal.TryParse(promoValue, out var val) ? val : 10,
                PromoType = promoType,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                IsActive = true,
                TargetAudience = "All"
            };

            Messages.Add(new ChatMessage 
            { 
                Content = response + $"\n\n System will create code: {promoCode}", 
                IsUser = false,
                IsProposal = true
            });
        }
        else
        {
            Messages.Add(new ChatMessage { Content = response, IsUser = false });
        }
    }

    private async void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingPromotion != null)
        {
            try 
            {
                await _promotionService.AddAsync(_pendingPromotion);
                Messages.Add(new ChatMessage { Content = " Promo code created successfully!", IsUser = false });
                _pendingPromotion = null;
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage { Content = " Error creating code: " + ex.Message, IsUser = false });
            }
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _pendingPromotion = null;
        Messages.Add(new ChatMessage { Content = "Action cancelled.", IsUser = false });
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        Messages.Clear();
        Messages.Add(new ChatMessage { Content = "Chat history cleared. How can I help you today?", IsUser = false });
    }
}
