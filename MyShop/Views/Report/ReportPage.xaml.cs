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
            RenderAllCharts();
        };
    }

    private void ReportPage_Loaded(object sender, RoutedEventArgs e)
    {
        RenderAllCharts();
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

    private void RenderAllCharts()
    {
        RenderPerformanceTrends();
        RenderRankingCharts();
    }

    private void RenderPerformanceTrends()
    {
        var canvas = TrendsCanvas;
        if (canvas == null) return;
        canvas.Children.Clear();

        var salesTrend = ViewModel.SalesTrend;
        if (!salesTrend.Any()) return;

        var width = Math.Max(800, salesTrend.Count * 60);
        canvas.Width = width;

        double marginStart = 40;
        double marginEnd = 80;
        double chartWidth = width - marginStart - marginEnd;
        double step = salesTrend.Count > 1 ? chartWidth / (salesTrend.Count - 1) : 0;

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

    private void RenderRankingCharts()
    {
        // 1. Categories Donut
        RenderDonutChart(CategoryChartCanvas, ViewModel.CategoryProfit.Select(p => new DonutPoint { Label = p.Category, Value = p.Amount, Color = p.Color }).ToList());

        // 2. Products Donut (assign colors to top 10)
        string[] palette = { "#3B82F6", "#10B981", "#A855F7", "#F59E0B", "#EF4444", "#06B6D4", "#84CC16", "#6366F1", "#EC4899", "#64748B" };
        var products = ViewModel.TopProducts.Take(10).ToList();
        var productPoints = products.Select((p, i) => new DonutPoint 
        { 
            Label = p.Name, 
            Value = (double)p.Amount, 
            Color = palette[i % palette.Length] 
        }).ToList();
        
        RenderDonutChart(RankingChartCanvas, productPoints);
    }

    private void RenderDonutChart(Canvas canvas, List<DonutPoint> points)
    {
        if (canvas == null) return;
        canvas.Children.Clear();
        if (!points.Any()) return;

        double total = points.Sum(p => p.Value);
        if (total == 0) return;

        double centerX = 125, centerY = 125, radius = 90, thickness = 30;
        double currentAngle = -90;

        foreach (var p in points)
        {
            double sweepAngle = (p.Value / total) * 360;
            if (sweepAngle >= 360) sweepAngle = 359.99;

            var path = CreateArc(centerX, centerY, radius, currentAngle, sweepAngle, p.Color, thickness);
            ToolTipService.SetToolTip(path, $"{p.Label}: {p.Value:N0}");
            canvas.Children.Add(path);

            currentAngle += sweepAngle;
        }

        // Center Text
        var totalLabel = new TextBlock
        {
            Width = 100,
            Text = "Total Share",
            FontSize = 12,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(centerX - 50, centerY - 20, 0, 0)
        };
        var totalValue = new TextBlock
        {
            Width = 100,
            Text = total > 1000000 ? $"${total / 1000000:N1}M" : $"${total:N0}",
            FontSize = 16,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(centerX - 50, centerY, 0, 0)
        };
        canvas.Children.Add(totalLabel);
        canvas.Children.Add(totalValue);
    }

    private Microsoft.UI.Xaml.Shapes.Path CreateArc(double cx, double cy, double r, double startAngle, double sweepAngle, string colorStr, double thickness)
    {
        var startPoint = DegreesToPoint(cx, cy, r, startAngle);
        var endPoint = DegreesToPoint(cx, cy, r, startAngle + sweepAngle);

        var color = ParseHex(colorStr);
        
        var path = new Microsoft.UI.Xaml.Shapes.Path
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = thickness,
            Data = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure
                    {
                        StartPoint = startPoint,
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment
                            {
                                Point = endPoint,
                                Size = new Windows.Foundation.Size(r, r),
                                SweepDirection = SweepDirection.Clockwise,
                                IsLargeArc = sweepAngle > 180
                            }
                        }
                    }
                }
            }
        };
        return path;
    }

    private Windows.Foundation.Point DegreesToPoint(double cx, double cy, double r, double angle)
    {
        double radians = angle * Math.PI / 180;
        return new Windows.Foundation.Point(cx + r * Math.Cos(radians), cy + r * Math.Sin(radians));
    }

    private Color ParseHex(string hex)
    {
        hex = hex.Replace("#", "");
        if (hex.Length == 6) hex = "FF" + hex;
        return Color.FromArgb(
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16),
            Convert.ToByte(hex.Substring(6, 2), 16));
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
            var y = 250 - (20 + normalized * 200);
            points.Add(new Windows.Foundation.Point(x, y));

            var hitBox = new Ellipse { Width = 24, Height = 24, Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent), Margin = new Thickness(x - 12, y - 12, 0, 0) };
            ToolTipService.SetToolTip(hitBox, $"{type} - {point.Label}: {point.Value:N0}");
            canvas.Children.Add(hitBox);

            var dot = new Ellipse { Width = 6, Height = 6, Fill = brush, Margin = new Thickness(x - 3, y - 3, 0, 0), IsHitTestVisible = false };
            canvas.Children.Add(dot);
        }

        var polyline = new Polyline { Points = points, Stroke = brush, StrokeThickness = 2, IsHitTestVisible = false };
        canvas.Children.Add(polyline);
    }

    private class DonutPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Color { get; set; } = "#CCCCCC";
    }
}
