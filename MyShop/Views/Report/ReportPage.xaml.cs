using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MyShop.ViewModels;
using Windows.UI;
using MyShop.Models;

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
            RenderPerformanceTrends();
        };
    }

    private void ReportPage_Loaded(object sender, RoutedEventArgs e)
    {
        RenderPerformanceTrends();
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

    private void RenderPerformanceTrends()
    {
        var canvas = TrendsCanvas;
        if (canvas == null) return;
        canvas.Children.Clear();

        var salesTrend = ViewModel.SalesTrend;
        var revenueTrend = ViewModel.RevenueTrend;
        var profitTrend = ViewModel.ProfitTrend;

        if (!salesTrend.Any()) return;

        // Dynamic width for scrollability
        var width = Math.Max(800, salesTrend.Count * 60);
        canvas.Width = width;

        // Spacing: Leave 40px at start and 80px at end (scroll bar margin)
        double marginStart = 40;
        double marginEnd = 80;
        double chartWidth = width - marginStart - marginEnd;
        double step = salesTrend.Count > 1 ? chartWidth / (salesTrend.Count - 1) : 0;

        // Draw ONLY the selected series
        switch (ViewModel.SelectedTrendType)
        {
            case "Revenue":
                DrawSeries(canvas, ViewModel.RevenueTrend, Color.FromArgb(0xFF, 0x10, 0xB9, 0x81), "Revenue", marginStart, step);
                break;
            case "Profit":
                DrawSeries(canvas, ViewModel.ProfitTrend, Color.FromArgb(0xFF, 0xA8, 0x55, 0xF7), "Profit", marginStart, step);
                break;
            default: // Sales
                DrawSeries(canvas, ViewModel.SalesTrend, Color.FromArgb(0xFF, 0x3B, 0x82, 0xF6), "Sales", marginStart, step);
                break;
        }

        // Draw Labels (only once)
        for (int i = 0; i < salesTrend.Count; i++)
        {
            var x = marginStart + (i * step);
            var label = new TextBlock
            {
                Text = salesTrend[i].Label,
                FontSize = 10,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                Margin = new Thickness(x - 20, 260, 0, 0),
                TextAlignment = TextAlignment.Center,
                Width = 40
            };
            canvas.Children.Add(label);
        }
    }

    private void DrawSeries(Canvas canvas, IList<TrendPoint> trend, Color color, string type, double marginStart, double step)
    {
        if (!trend.Any()) return;

        var max = trend.Max(x => x.Value);
        if (max == 0) max = 1;

        var points = new PointCollection();
        var brush = new SolidColorBrush(color);

        for (int i = 0; i < trend.Count; i++)
        {
            var point = trend[i];
            var normalized = point.Value / max;
            var x = marginStart + (i * step);
            var y = 250 - (20 + normalized * 200); // 250 is bottom-ish, 20 bottom margin, 200 height
            points.Add(new Windows.Foundation.Point(x, y));

            // Tooltip Hitbox
            var hitBox = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                Margin = new Thickness(x - 12, y - 12, 0, 0)
            };
            ToolTipService.SetToolTip(hitBox, $"{type} - {point.Label}: {point.Value:N0}");
            canvas.Children.Add(hitBox);

            // Visible dot
            var dot = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = brush,
                Margin = new Thickness(x - 3, y - 3, 0, 0),
                IsHitTestVisible = false
            };
            canvas.Children.Add(dot);
        }

        var polyline = new Polyline
        {
            Points = points,
            Stroke = brush,
            StrokeThickness = 2,
            IsHitTestVisible = false
        };
        canvas.Children.Add(polyline);
    }
}
