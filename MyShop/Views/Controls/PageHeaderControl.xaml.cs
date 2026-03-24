using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views.Controls;

public sealed partial class PageHeaderControl : UserControl
{
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageHeaderControl), new PropertyMetadata(""));

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(PageHeaderControl), new PropertyMetadata(""));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(PageHeaderControl), new PropertyMetadata(""));

    public UIElement Actions
    {
        get => (UIElement)GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public static readonly DependencyProperty ActionsProperty =
        DependencyProperty.Register(nameof(Actions), typeof(UIElement), typeof(PageHeaderControl), new PropertyMetadata(null));

    public PageHeaderControl()
    {
        this.InitializeComponent();
    }
}
