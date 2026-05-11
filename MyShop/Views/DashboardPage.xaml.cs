using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
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
        ViewModel = App.Services.GetRequiredService<DashboardViewModel>();
        this.Loaded += DashboardPage_Loaded;
    }

    private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Cập nhật ngày tháng hiển thị
        DateLabel.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy", new System.Globalization.CultureInfo("vi-VN"));
        ChartMonthLabel.Text = $"Doanh thu Tháng {DateTime.Now.Month}/{DateTime.Now.Year}";

        // Lắng nghe khi dữ liệu trong ViewModel thay đổi thì vẽ lại biểu đồ
        ViewModel.PropertyChanged += (s, args) => {
            if (args.PropertyName == nameof(ViewModel.RevenueTrend))
            {
                RenderRevenueChart();
            }
        };
        
        RenderRevenueChart();
    }

    private void RenderRevenueChart()
    {
       var trend = ViewModel.RevenueTrend.ToList();
    if (!trend.Any() || RevenueChartCanvas == null) return;

    var total = trend.Sum(x => (long)x.Value);
    ChartTotalLabel.Text = $"Tổng tháng: {total:N0} VND";
    
        var canvas = RevenueChartCanvas;
        canvas.Children.Clear();

        double canvasWidth = canvas.ActualWidth > 0 ? canvas.ActualWidth : 760;
        double canvasHeight = 200;

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
            // SỬA: Dùng Color.FromArgb (cần using Windows.UI)
            Fill = new SolidColorBrush(Color.FromArgb(30, 187, 77, 0)),
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
            Stroke = new SolidColorBrush(Color.FromArgb(255, 187, 77, 0)),
            StrokeThickness = 2.5,
            StrokeLineJoin = PenLineJoin.Round
        };
        canvas.Children.Add(line);
        ChartLabelsPanel.Children.Clear();
        // Dots
        for (int i = 0; i < count; i++)
        {
            double x = padLeft + i * stepX;
            double y = padTop + chartH - (values[i] / max) * chartH;

            var dot = new Ellipse
            {
                Width = 7,
                Height = 7,
                Fill = new SolidColorBrush(Color.FromArgb(255, 187, 77, 0)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(dot, x - 3.5);
            Canvas.SetTop(dot, y - 3.5);
            canvas.Children.Add(dot);
        }

        // X-axis labels
        ChartLabelsPanel.Children.Clear();
        for (int i = 0; i < count; i++)
        {
            bool show = (i == 0) || (i == count - 1) || (i % 5 == 4);
            var lbl = new TextBlock
            {
                Text = show ? trend[i].Label : "",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = count > 1 ? chartW / (count - 1) : chartW
            };
            ChartLabelsPanel.Children.Add(lbl);
        }
    }
}
