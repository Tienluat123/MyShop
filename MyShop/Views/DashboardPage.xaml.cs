using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using MyShop.ViewModels;
using MyShop.Models;
using MyShop.Views.Pos.Components;

namespace MyShop.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<DashboardViewModel>();
        this.Loaded += DashboardPage_Loaded;
        this.Unloaded += DashboardPage_Unloaded;
    }

    private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Cập nhật ngày tháng hiển thị
        DateLabel.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy", new System.Globalization.CultureInfo("vi-VN"));
        ChartMonthLabel.Text = $"Revenue Month {DateTime.Now.Month}/{DateTime.Now.Year}";

        // Đăng ký sự kiện (đảm bảo không bị lặp)
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Tải lại dữ liệu mỗi khi vào trang
        _ = ViewModel.LoadDashboardData();
        
        if (RevenueChartCanvas != null)
        {
            RevenueChartCanvas.SizeChanged -= RevenueChartCanvas_SizeChanged;
            RevenueChartCanvas.SizeChanged += RevenueChartCanvas_SizeChanged;
        }

        RenderRevenueChart();
    }

    private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

        if (RevenueChartCanvas != null)
        {
            RevenueChartCanvas.SizeChanged -= RevenueChartCanvas_SizeChanged;
        }
    }

    private void RevenueChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
        {
            return;
        }

        RenderRevenueChart();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.RevenueTrend))
        {
            RenderRevenueChart();
        }
    }

    private async void RecentOrders_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Order order)
        {
            var orderManagementVM = App.Services.GetRequiredService<OrderManagementViewModel>();
            await orderManagementVM.SelectOrder(order);
            
            var detailModal = new OrderDetailModal();
            detailModal.DataContext = orderManagementVM;
            
            if (ModalContainer != null)
            {
                ModalContainer.Children.Clear();
                ModalContainer.Children.Add(detailModal);
                ModalContainer.Visibility = Visibility.Visible;
                
                orderManagementVM.CloseOrderDetailAction = () => {
                    ModalContainer.Visibility = Visibility.Collapsed;
                    ModalContainer.Children.Clear();
                };
            }
        }
    }

    private void RenderRevenueChart()
    {
        var trend = ViewModel.RevenueTrend.ToList();
        if (!trend.Any() || RevenueChartCanvas == null) return;

        var total = trend.Sum(x => (long)x.Value);
        ChartTotalLabel.Text = $"Monthly total: {total:N0} VND";
    
        var canvas = RevenueChartCanvas;
        canvas.Children.Clear();

        double canvasWidth = canvas.ActualWidth;
        double canvasHeight = canvas.ActualHeight;

        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            return;
        }

        var values = trend.Select(x => (double)x.Value).ToArray();
        var max = values.Max();
        if (max == 0) max = 1;

        double padLeft = 10;
        double padRight = 10;
        double padTop = 10;
        double padBottom = 30;
        double chartW = canvasWidth - padLeft - padRight;
        double chartH = canvasHeight - padTop - padBottom;
        int count = trend.Count;
        double stepX = count > 1 ? chartW / (count - 1) : chartW;

        // Gradient area
        var areaPoints = new Microsoft.UI.Xaml.Media.PointCollection();
        areaPoints.Add(new Windows.Foundation.Point(padLeft, canvasHeight - padBottom));

        for (int i = 0; i < count; i++)
        {
            double x = padLeft + i * stepX;
            double y = padTop + chartH - (values[i] / max) * chartH;
            areaPoints.Add(new Windows.Foundation.Point(x, y));
        }
        areaPoints.Add(new Windows.Foundation.Point(padLeft + (count - 1) * stepX, canvasHeight - padBottom));

        var area = new Polygon
        {
            Points = areaPoints,
            // SỬA: Dùng Windows.UI.Color.FromArgb (cần using Windows.UI)
            Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(30, 187, 77, 0)),
            Stroke = null
        };
        canvas.Children.Add(area);

        // Line
        // SỬA: Thay Windows.UI.Xaml.Media thành Microsoft.UI.Xaml.Media
        var linePoints = new Microsoft.UI.Xaml.Media.PointCollection();
        for (int i = 0; i < count; i++)
        {
            double x = padLeft + i * stepX;
            double y = padTop + chartH - (values[i] / max) * chartH;
            linePoints.Add(new Windows.Foundation.Point(x, y));
        }

        var line = new Polyline
        {
            Points = linePoints,
            Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 187, 77, 0)),
            StrokeThickness = 2.5
        };
        canvas.Children.Add(line);
        ChartLabelsPanel.Children.Clear();
        // Dots and X-axis labels (Vẽ trực tiếp lên Canvas để khớp vị trí)
        for (int i = 0; i < count; i++)
        {
            double x = padLeft + i * stepX;
            double y = padTop + chartH - (values[i] / max) * chartH;

            // 1. Vẽ chấm tròn
            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 187, 77, 0)),
                Stroke = new SolidColorBrush(Microsoft.UI.Colors.White),
                StrokeThickness = 2
            };
            
            // Fix closure capture for loop variables
            var currentLabel = trend[i].Label;
            var currentAmount = values[i];

            // Hiện số tiền khi hover
            ToolTipService.SetToolTip(dot, $"Day {currentLabel}: {currentAmount:N0} VND");
            
            // Hiệu ứng hover
            dot.PointerEntered += (s, e) => {
                dot.Width = 12;
                dot.Height = 12;
                Canvas.SetLeft(dot, x - 6);
                Canvas.SetTop(dot, y - 6);
                dot.StrokeThickness = 3;
            };
            dot.PointerExited += (s, e) => {
                dot.Width = 8;
                dot.Height = 8;
                Canvas.SetLeft(dot, x - 4);
                Canvas.SetTop(dot, y - 4);
                dot.StrokeThickness = 2;
            };

            // Bấm vào hiện Flyout
            dot.Tapped += (s, e) => {
                var flyoutContent = new StackPanel { Padding = new Thickness(12), Spacing = 4 };
                flyoutContent.Children.Add(new TextBlock { Text = $"Day {currentLabel}", FontSize = 12, Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray) });
                flyoutContent.Children.Add(new TextBlock { Text = $"{currentAmount:N0} VND", FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 187, 77, 0)) });
                
                var flyout = new Flyout { Content = flyoutContent };
                flyout.ShowAt(dot);
            };

            Canvas.SetLeft(dot, x - 4);
            Canvas.SetTop(dot, y - 4);
            canvas.Children.Add(dot);

            // 2. Vẽ nhãn ngày ngay dưới chấm tròn
            var lbl = new TextBlock
            {
                Text = trend[i].Label,
                FontSize = 9,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 114, 128)),
                Width = 20,
                TextAlignment = TextAlignment.Center
            };
            canvas.Children.Add(lbl);
            Canvas.SetLeft(lbl, x - 10); // Căn giữa nhãn (20/2 = 10)
            Canvas.SetTop(lbl, canvasHeight - 20); // Đưa xuống đáy canvas
        }
        ChartLabelsPanel.Children.Clear(); // Không dùng Panel này nữa để tránh lệch
    }
}

