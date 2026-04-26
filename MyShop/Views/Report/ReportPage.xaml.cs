using System;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MyShop.ViewModels;
using Windows.UI;

namespace MyShop.Views.Report;

public sealed partial class ReportPage : Page
{
    public ReportViewModel ViewModel { get; }

    public ReportPage()
    {
        this.InitializeComponent();
        ViewModel = (App.Services as MyShop.Di.ServiceContainer)!.Get<ReportViewModel>();
        this.DataContext = ViewModel;
        this.Loaded += ReportPage_Loaded;
        ViewModel.RequestChartRefresh += (s, e) => {
            RenderSalesTrend();
            RenderRevenueChart();
        };
    }

    private void ReportPage_Loaded(object sender, RoutedEventArgs e)
    {
        RenderSalesTrend();
        RenderRevenueChart();
    }

    private async void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        // Sync dates based on granularity
        if (ViewModel.SelectedPeriod == "Month")
        {
            ViewModel.StartDate = new DateTime(ViewModel.StartYear, ViewModel.StartMonth, 1);
            ViewModel.EndDate = new DateTime(ViewModel.EndYear, ViewModel.EndMonth, 1).AddMonths(1).AddDays(-1);
        }
        else if (ViewModel.SelectedPeriod == "Year")
        {
            ViewModel.StartDate = new DateTime(ViewModel.StartYear, 1, 1);
            ViewModel.EndDate = new DateTime(ViewModel.EndYear, 12, 31);
        }

        await ViewModel.RefreshData();
    }

    private void RenderSalesTrend()
    {
        var trend = ViewModel.SalesTrend;
        if (!trend.Any()) return;

        var color = Color.FromArgb(0xFF, 0x3B, 0x82, 0xF6); // Blue-500
        var max = trend.Max(x => x.Value);
        if (max == 0) max = 1;

        var canvas = SalesTrendCanvas;
        canvas.Children.Clear();

        var points = new PointCollection();
        var width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 400; // Fallback width
        var step = width / (trend.Count - 1);

        for (int i = 0; i < trend.Count; i++)
        {
            var point = trend[i];
            var normalized = point.Value / max;
            var x = i * step;
            var y = 200 - (20 + normalized * 160);
            points.Add(new Windows.Foundation.Point(x, y));

            // Small dot at each point
            var dot = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(color),
                Margin = new Thickness(x - 3, y - 3, 0, 0)
            };
            canvas.Children.Add(dot);
            
            // Label
            var label = new TextBlock
            {
                Text = point.Label,
                FontSize = 10,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                Margin = new Thickness(x - 10, 180, 0, 0)
            };
            canvas.Children.Add(label);
        }

        var polyline = new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2
        };
        canvas.Children.Add(polyline);
    }

    private void RenderRevenueChart()
    {
        var data = ViewModel.RevenueData;
        if (!data.Any()) return;

        var color = Color.FromArgb(0xFF, 0x10, 0xB9, 0x81); // Emerald-500
        var max = data.Max(x => x.Value);
        if (max == 0) max = 1;

        var panel = RevenueBarPanel;
        panel.Children.Clear();

        foreach (var point in data)
        {
            var normalized = point.Value / max;
            var height = 20 + normalized * 140;

            var bar = new Border
            {
                Width = 40,
                Height = height,
                Background = new SolidColorBrush(color),
                CornerRadius = new CornerRadius(4, 4, 0, 0),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            ToolTipService.SetToolTip(bar, $"{point.Label}: {point.Value:N0}");

            var container = new StackPanel
            {
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            container.Children.Add(bar);
            container.Children.Add(new TextBlock
            {
                Text = point.Label,
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            panel.Children.Add(container);
        }
    }
}
