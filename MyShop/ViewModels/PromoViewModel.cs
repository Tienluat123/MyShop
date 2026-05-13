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

    [ObservableProperty]
    private Customer? _customer;

    public ObservableCollection<Promotion> ActivePromotions { get; } = new();

    public event Action<Promotion?>? PromotionSelected;

    public PromoViewModel(IPromotionService promoService)
    {
        _promoService = promoService;
    }

    public async Task LoadActivePromotions()
    {
        var result = await _promoService.GetAllAsync();
        ActivePromotions.Clear();
        foreach (var p in result.Items)
        {
            if (_promoService.IsPromotionApplicable(p, OrderTotal, Customer, out _))
            {
                ActivePromotions.Add(p);
            }
        }
    }

    [RelayCommand]
    private async Task ApplyCode()
    {
        if (string.IsNullOrWhiteSpace(PromoCodeEntry)) return;

        ErrorMessage = string.Empty;
        var result = await _promoService.ApplyPromotionAsync(PromoCodeEntry, OrderTotal, Customer);
        if (result.Success)
        {
            var promo = await _promoService.GetByCodeAsync(PromoCodeEntry);
            PromotionSelected?.Invoke(promo);
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Promotion code is invalid or not eligible.";
        }
    }

    [RelayCommand]
    private void SelectPromotion(Promotion? promo)
    {
        if (promo != null && !_promoService.IsPromotionApplicable(promo, OrderTotal, Customer, out var errorMessage))
        {
            ErrorMessage = errorMessage ?? "Promotion code is invalid or not eligible.";
            return;
        }

        ErrorMessage = string.Empty;
        PromotionSelected?.Invoke(promo);
    }

    [RelayCommand]
    private void Skip()
    {
        PromotionSelected?.Invoke(null);
    }
}
