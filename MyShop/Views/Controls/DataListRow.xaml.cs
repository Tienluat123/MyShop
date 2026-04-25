using Microsoft.UI.Xaml.Controls;

namespace MyShop.Views.Controls;

public sealed partial class DataListRow : UserControl
{
    public UIElement Avatar
    {
        get => (UIElement)GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    public static readonly DependencyProperty AvatarProperty =
        DependencyProperty.Register(nameof(Avatar), typeof(UIElement), typeof(DataListRow), new PropertyMetadata(null));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(DataListRow), new PropertyMetadata(""));

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(DataListRow), new PropertyMetadata(""));

    public UIElement Content
    {
        get => (UIElement)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(DataListRow), new PropertyMetadata(null));

    public UIElement Actions
    {
        get => (UIElement)GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public static readonly DependencyProperty ActionsProperty =
        DependencyProperty.Register(nameof(Actions), typeof(UIElement), typeof(DataListRow), new PropertyMetadata(null));

    public DataListRow()
    {
        this.InitializeComponent();
    }
}
