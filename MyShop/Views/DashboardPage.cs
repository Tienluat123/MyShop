using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MyShop.ViewModels;
using Windows.UI;

namespace MyShop.Views;

public sealed class DashboardPage : Page
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage()
    {
        _viewModel = new DashboardViewModel();
        DataContext = _viewModel;
        Content = BuildDashboardLayout();
    }

    private UIElement BuildDashboardLayout()
    {
        var root = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Padding = new Thickness(16),
                Spacing = 16,
                Children =
                {
                    CreateSectionHeader("DASHBOARD"),
                    CreateMetricCards(),
                    CreateTrendsPanel(),
                    CreateTopProductsSection()
                }
            }
        };
        return root;
    }

    private TextBlock CreateSectionHeader(string title)
    {
        return new TextBlock
        {
            Text = title,
            FontSize = 24,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        };
    }

    private Grid CreateMetricCards()
    {
        var grid = new Grid { ColumnSpacing = 12 };
        for (var i = 0; i < 4; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var metrics = new[]
        {
            (Title: "Sản phẩm", Value: _viewModel.ProductsCount.ToString("N0"), Subtitle: "Tổng số sản phẩm"),
            (Title: "Doanh thu", Value: _viewModel.Revenue.ToString("N0"), Subtitle: "Tổng doanh thu (triệu)"),
            (Title: "Đơn hàng", Value: _viewModel.OrdersCount.ToString("N0"), Subtitle: "Số đơn hàng"),
            (Title: "Khách", Value: _viewModel.CustomersCount.ToString("N0"), Subtitle: "Khách hàng mới")
        };

        var colors = new[]
        {
            Color.FromArgb(0xFF, 0xFF, 0x69, 0xB4), // HotPink
            Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF), // DodgerBlue
            Color.FromArgb(0xFF, 0x32, 0xCD, 0x32), // LimeGreen
            Color.FromArgb(0xFF, 0xDA, 0xA5, 0x20)  // Goldenrod
        };

        for (var i = 0; i < metrics.Length; i++)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0xE7, 0xEB)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Child = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock { Text = metrics[i].Title, FontSize = 14, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80)) },
                        new TextBlock { Text = metrics[i].Value, FontSize = 28, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(colors[i]) },
                        new TextBlock { Text = metrics[i].Subtitle, FontSize = 12, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80)) }
                    }
                }
            };

            grid.Children.Add(card);
            Grid.SetColumn(card, i);
        }

        return grid;
    }

    private Grid CreateTrendsPanel()
    {
        var panel = new Grid { ColumnSpacing = 12 };
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        panel.Children.Add(CreateTrendCard("Sản phẩm", "Số lượng sản phẩm", _viewModel.ProductTrend, Color.FromArgb(0xFF, 0xFF, 0x14, 0x93), true));
        panel.Children.Add(CreateTrendCard("Doanh thu", "Triệu VND", _viewModel.RevenueTrend, Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF), false));

        Grid.SetColumn(panel.Children[0], 0);
        Grid.SetColumn(panel.Children[1], 1);

        return panel;
    }

    private Border CreateTrendCard(string title, string subtitle, IEnumerable<DashboardViewModel.TrendPoint> trend, Color color, bool isLineChart = false)
    {
        var values = trend.Select(x => x.Value).ToArray();
        var max = values.Any() ? values.Max() : 1;
        if (max == 0) max = 1;

        UIElement chartElement;
        if (isLineChart)
        {
            var canvas = new Canvas { Height = 200, Width = 300 };
            var points = new PointCollection();
            var trendList = trend.ToList();
            for (int i = 0; i < trendList.Count; i++)
            {
                var point = trendList[i];
                var normalized = (double)point.Value / max;
                var x = i * 30.0;
                var y = 200 - (80 + normalized * 120);
                points.Add(new Windows.Foundation.Point(x, y));
            }
            var polyline = new Polyline
            {
                Points = points,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 3
            };
            canvas.Children.Add(polyline);
            chartElement = canvas;
        }
        else
        {
            var chartBars = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 6
            };

            foreach (var point in trend)
            {
                var normalized = (double)point.Value / max;
                var height = 80 + normalized * 120;
                var bar = new Border
                {
                    Width = 24,
                    Height = height,
                    Background = new SolidColorBrush(color),
                    CornerRadius = new CornerRadius(12),
                    VerticalAlignment = VerticalAlignment.Bottom
                };

                var stacked = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center };
                stacked.Children.Add(bar);
                stacked.Children.Add(new TextBlock { Text = point.Label, FontSize = 10, Margin = new Thickness(0, 6, 0, 0), HorizontalAlignment = HorizontalAlignment.Center });
                chartBars.Children.Add(stacked);
            }
            chartElement = chartBars;
        }

        return new Border
        {
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0xE7, 0xEB)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = title, FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                    new TextBlock { Text = subtitle, FontSize = 12, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80)) },
                    new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(0x0F, color.R, color.G, color.B)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(8),
                        Child = chartElement
                    }
                }
            }
        };
    }

    private Border CreateTopProductsSection()
    {
        var topProductList = new ListView
        {
            ItemsSource = _viewModel.TopProducts,
            SelectionMode = ListViewSelectionMode.None,
            Margin = new Thickness(0, 8, 0, 0),
            ItemTemplate = new DataTemplate(() =>
            {
                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    Padding = new Thickness(10)
                };

                var name = new TextBlock { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Medium, VerticalAlignment = VerticalAlignment.Center };
                name.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Name") });

                var amount = new TextBlock { FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center };
                amount.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Amount") });

                grid.Children.Add(name);
                grid.Children.Add(amount);
                Grid.SetColumn(amount, 1);
                return grid;
            })
        };

        return new Border
        {
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0xE7, 0xEB)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "TOP 5 sản phẩm", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold },
                    topProductList
                }
            }
        };
    }
}
