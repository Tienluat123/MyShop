using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Views.Report;

public sealed partial class MetricCard : UserControl
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(MetricCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(MetricCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(string), typeof(MetricCard), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
        nameof(Color), typeof(Brush), typeof(MetricCard), new PropertyMetadata(null));

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public Brush Color { get => (Brush)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }

    public Brush IconBackground => Color ?? (Brush)Application.Current.Resources["PrimaryColor"];

    public MetricCard()
    {
        this.InitializeComponent();
    }
}
