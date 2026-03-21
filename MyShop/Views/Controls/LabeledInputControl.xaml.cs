using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views.Controls;

public sealed partial class LabeledInputControl : UserControl
{
    public LabeledInputControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(LabeledInputControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty InputContentProperty = DependencyProperty.Register(
        nameof(InputContent),
        typeof(object),
        typeof(LabeledInputControl),
        new PropertyMetadata(null));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object InputContent
    {
        get => GetValue(InputContentProperty);
        set => SetValue(InputContentProperty, value);
    }
}
