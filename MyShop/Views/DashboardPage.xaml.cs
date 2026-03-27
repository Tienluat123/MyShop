using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MyShop.ViewModels;
using Windows.UI;

namespace MyShop.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        this.InitializeComponent();
        ViewModel = new DashboardViewModel();
        this.Loaded += DashboardPage_Loaded;
    }

    private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        RenderProductTrend();
        RenderRevenueTrend();
    }

    private void RenderProductTrend()
    {
        var trend = ViewModel.ProductTrend;
        var color = Color.FromArgb(0xFF, 0xFF, 0x14, 0x93);
        
        var values = trend.Select(x => x.Value).ToArray();
        var max = values.Any() ? values.Max() : 1;
        if (max == 0) max = 1;

        var canvas = ProductTrendCanvas;
        canvas.Children.Clear();

        var points = new PointCollection();
        var trendList = trend.ToList();
        for (int i = 0; i < trendList.Count; i++)
        {
            var point = trendList[i];
            var normalized = (double)point.Value / max;
            var x = i * 40.0;
            var y = 200 - (20 + normalized * 160);
            points.Add(new Windows.Foundation.Point(x, y));
        }

        var polyline = new Polyline
        {
            Points = points,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 3
        };
        canvas.Children.Add(polyline);
    }

    private void RenderRevenueTrend()
    {
        var trend = ViewModel.RevenueTrend;
        var color = Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF);
        
        var values = trend.Select(x => x.Value).ToArray();
        var max = values.Any() ? values.Max() : 1;
        if (max == 0) max = 1;

        var panel = RevenueTrendPanel;
        panel.Children.Clear();

        foreach (var point in trend)
        {
            var normalized = (double)point.Value / max;
            var height = 40 + normalized * 140;
            
            var bar = new Border
            {
                Width = 24,
                Height = height,
                Background = new SolidColorBrush(color),
                CornerRadius = new CornerRadius(12),
                VerticalAlignment = VerticalAlignment.Bottom
            };

            var stacked = new StackPanel 
            { 
                Orientation = Orientation.Vertical, 
                VerticalAlignment = VerticalAlignment.Bottom, 
                HorizontalAlignment = HorizontalAlignment.Center 
            };
            stacked.Children.Add(bar);
            stacked.Children.Add(new TextBlock 
            { 
                Text = point.Label, 
                FontSize = 10, 
                Margin = new Thickness(0, 6, 0, 0), 
                HorizontalAlignment = HorizontalAlignment.Center 
            });
            
            panel.Children.Add(stacked);
        }
    }
}
