using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.ViewModels;

public partial class PromoViewModel : ObservableObject
{
    private readonly IPromotionService _promoService;

    [ObservableProperty]
    private string _promoCodeEntry = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private decimal _orderTotal;

    [ObservableProperty]
    private string? _customerTier;

    public ObservableCollection<Promotion> ActivePromotions { get; } = new();

    public event Action<Promotion?>? PromotionSelected;

    public PromoViewModel(IPromotionService promoService)
    {
        _promoService = promoService;
        _ = LoadActivePromotions();
    }

    private async Task LoadActivePromotions()
    {
        var result = await _promoService.GetAllAsync();
        ActivePromotions.Clear();
        foreach (var p in result.Items)
        {
            ActivePromotions.Add(p);
        }
    }

    [RelayCommand]
    private async Task ApplyCode()
    {
        if (string.IsNullOrWhiteSpace(PromoCodeEntry)) return;

        var result = await _promoService.ApplyPromotionAsync(PromoCodeEntry, OrderTotal, CustomerTier);
        if (result.Success)
        {
            var promo = await _promoService.GetByCodeAsync(PromoCodeEntry);
            PromotionSelected?.Invoke(promo);
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Mã khuyến mãi không hợp lệ hoặc không đủ điều kiện.";
        }
    }

    [RelayCommand]
    private void SelectPromotion(Promotion? promo)
    {
        PromotionSelected?.Invoke(promo);
    }

    [RelayCommand]
    private void Skip()
    {
        PromotionSelected?.Invoke(null);
    }
}
