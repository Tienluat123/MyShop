using System;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public sealed class LoyaltyRulesViewModel : BaseViewModel
{
    private readonly Action onBack;
    private int pointsRate;
    private int moneyUnit;

    public LoyaltyRulesViewModel(Action onBack)
    {
        this.onBack = onBack;
        PointsRate = 1;
        MoneyUnit = 10000;

        TierThresholdsCollection =
        [
            new TierThreshold { TierName = "Silver Tier", Points = 500 },
            new TierThreshold { TierName = "Gold Tier", Points = 1500 },
            new TierThreshold { TierName = "VIP Tier", Points = 5000 }
        ];

        SaveRulesCommand = new RelayCommand(SaveRules);
        BackCommand = new RelayCommand(() => this.onBack());
    }

    public int PointsRate
    {
        get => pointsRate;
        set
        {
            if (pointsRate != value)
            {
                pointsRate = value;
                OnPropertyChanged();
            }
        }
    }

    public int MoneyUnit
    {
        get => moneyUnit;
        set
        {
            if (moneyUnit != value)
            {
                moneyUnit = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<TierThreshold> TierThresholdsCollection { get; }

    public RelayCommand SaveRulesCommand { get; }
    public RelayCommand BackCommand { get; }

    private void SaveRules()
    {
        onBack();
    }
}
