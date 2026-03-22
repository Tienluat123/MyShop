using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public DashboardViewModel()
    {
        // Mock metrics
        ProductsCount = 7460;
        Revenue = 42200;
        OrdersCount = 1280;
        CustomersCount = 980;

        ProductTrend = new ObservableCollection<TrendPoint>
        {
            new TrendPoint("27/1", 1200),
            new TrendPoint("28/1", 2500),
            new TrendPoint("29/1", 1800),
            new TrendPoint("30/1", 4200),
            new TrendPoint("31/1", 4000),
            new TrendPoint("1/2", 5200)
        };

        RevenueTrend = new ObservableCollection<TrendPoint>
        {
            new TrendPoint("27/1", 800),
            new TrendPoint("28/1", 1700),
            new TrendPoint("29/1", 2900),
            new TrendPoint("30/1", 3600),
            new TrendPoint("31/1", 1800)
        };

        TopProducts = new ObservableCollection<TopProduct>
        {
            new TopProduct("Caramel Latte", 500),
            new TopProduct("Croissant", 450),
            new TopProduct("Matcha Latte", 380),
            new TopProduct("Blueberry Muffin", 310),
            new TopProduct("Flat White", 210)
        };
    }

    [ObservableProperty]
    private int productsCount;

    [ObservableProperty]
    private int revenue;

    [ObservableProperty]
    private int ordersCount;

    [ObservableProperty]
    private int customersCount;

    public ObservableCollection<TrendPoint> ProductTrend { get; }
    public ObservableCollection<TrendPoint> RevenueTrend { get; }
    public ObservableCollection<TopProduct> TopProducts { get; }

    public class TrendPoint
    {
        public TrendPoint(string label, int value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }
        public int Value { get; }
    }

    public class TopProduct
    {
        public TopProduct(string name, int amount)
        {
            Name = name;
            Amount = amount;
        }

        public string Name { get; }
        public int Amount { get; }
    }
}
