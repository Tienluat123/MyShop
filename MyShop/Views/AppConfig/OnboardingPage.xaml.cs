using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Views.AppConfig;

public sealed partial class OnboardingPage : Page
{
    private readonly SolidColorBrush _activeBackground = new(Windows.UI.Color.FromArgb(255, 234, 88, 12));
    private readonly SolidColorBrush _inactiveBackground = new(Windows.UI.Color.FromArgb(255, 229, 231, 235));
    private readonly SolidColorBrush _activeText = new(Colors.White);
    private readonly SolidColorBrush _inactiveText = new(Windows.UI.Color.FromArgb(255, 55, 65, 81));

    private int _currentStep = 1;

    public event EventHandler? ConfigureRequested;

    public OnboardingPage()
    {
        InitializeComponent();
        ShowStep(1);
    }

    private void StepOneButton_Click(object sender, RoutedEventArgs e)
    {
        ShowStep(1);
    }

    private void StepTwoButton_Click(object sender, RoutedEventArgs e)
    {
        ShowStep(2);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowStep(1);
    }

    private void PrimaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep == 1)
        {
            ShowStep(2);
            return;
        }

        ConfigureRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ShowStep(int step)
    {
        _currentStep = step;

        var isStepOne = step == 1;
        StepOneMainPanel.Visibility = isStepOne ? Visibility.Visible : Visibility.Collapsed;
        StepOneSidePanel.Visibility = isStepOne ? Visibility.Visible : Visibility.Collapsed;
        StepTwoMainPanel.Visibility = isStepOne ? Visibility.Collapsed : Visibility.Visible;
        StepTwoSidePanel.Visibility = isStepOne ? Visibility.Collapsed : Visibility.Visible;

        StepOneButton.Background = isStepOne ? _activeBackground : _inactiveBackground;
        StepOneNumber.Foreground = isStepOne ? _activeText : _inactiveText;
        StepOneText.Foreground = isStepOne ? _activeText : _inactiveText;

        StepTwoButton.Background = isStepOne ? _inactiveBackground : _activeBackground;
        StepTwoNumber.Foreground = isStepOne ? _inactiveText : _activeText;
        StepTwoText.Foreground = isStepOne ? _inactiveText : _activeText;

        BackButton.Visibility = isStepOne ? Visibility.Collapsed : Visibility.Visible;
        PrimaryButtonText.Text = isStepOne ? "Continue" : "Start configuration";
        FooterText.Text = isStepOne
            ? "Next: preview the main screens before configuring the database."
            : "Ready: open database configuration, click Test to check the connection, then save.";
    }

    private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout(e.NewSize.Width);
    }

    private void ApplyResponsiveLayout(double width)
    {
        var compact = width < 860;
        var veryNarrow = width < 560;

        HeaderContent.Margin = new Thickness(compact ? 16 : 24, 0, compact ? 16 : 24, 0);
        HeaderTitlePanel.Visibility = veryNarrow ? Visibility.Collapsed : Visibility.Visible;

        ContentShell.MaxWidth = compact ? 760 : 1160;
        ContentShell.Margin = compact
            ? new Thickness(16, 22, 16, 22)
            : new Thickness(24, 28, 24, 28);

        MainContentColumn.Width = new GridLength(1, GridUnitType.Star);
        ContentGapColumn.Width = compact ? new GridLength(0) : new GridLength(32);
        SideContentColumn.Width = compact ? new GridLength(0) : new GridLength(0.88, GridUnitType.Star);
        MobileContentGapRow.Height = compact ? new GridLength(20) : new GridLength(0);

        MoveSidePanel(StepOneSidePanel, compact);
        MoveSidePanel(StepTwoSidePanel, compact);

        StepOneTitle.FontSize = compact ? 30 : 38;
        StepTwoTitle.FontSize = compact ? 30 : 38;
        StepOneDescription.FontSize = compact ? 14 : 16;
        StepTwoDescription.FontSize = compact ? 14 : 16;

        ApplyConfigCardsLayout(compact);
        ApplyFeatureCardsLayout(compact);
        ApplyFooterLayout(compact, veryNarrow);
    }

    private static void MoveSidePanel(FrameworkElement panel, bool compact)
    {
        Grid.SetColumn(panel, compact ? 0 : 2);
        Grid.SetRow(panel, compact ? 2 : 0);
    }

    private void ApplyConfigCardsLayout(bool compact)
    {
        ConfigCardsSecondColumn.Width = compact ? new GridLength(0) : new GridLength(1, GridUnitType.Star);

        SetCardPosition(ServerCard, 0, 0);
        SetCardPosition(DatabaseCard, compact ? 1 : 0, compact ? 0 : 1);
        SetCardPosition(AccountCard, compact ? 2 : 1, 0);
        SetCardPosition(SaveCard, compact ? 3 : 1, compact ? 0 : 1);
    }

    private void ApplyFeatureCardsLayout(bool compact)
    {
        FeatureCardsSecondColumn.Width = compact ? new GridLength(0) : new GridLength(1, GridUnitType.Star);

        SetCardPosition(DashboardFeatureCard, 0, 0);
        SetCardPosition(PosFeatureCard, compact ? 1 : 0, compact ? 0 : 1);
        SetCardPosition(ProductsFeatureCard, compact ? 2 : 1, 0);
        SetCardPosition(CustomersFeatureCard, compact ? 3 : 1, compact ? 0 : 1);
        SetCardPosition(PromotionsFeatureCard, compact ? 4 : 2, 0);
        SetCardPosition(StaffFeatureCard, compact ? 5 : 2, compact ? 0 : 1);
        SetCardPosition(ReportsFeatureCard, compact ? 6 : 3, 0);
        SetCardPosition(SettingsFeatureCard, compact ? 7 : 3, compact ? 0 : 1);
    }

    private static void SetCardPosition(FrameworkElement card, int row, int column)
    {
        Grid.SetRow(card, row);
        Grid.SetColumn(card, column);
    }

    private void ApplyFooterLayout(bool compact, bool veryNarrow)
    {
        FooterContent.Margin = compact
            ? new Thickness(16, 12, 16, 12)
            : new Thickness(24, 14, 24, 14);
        FooterText.Margin = compact
            ? new Thickness(0, 0, 0, 0)
            : new Thickness(0, 0, 28, 0);

        FooterActionsRow.Height = compact ? new GridLength(12) : new GridLength(0);
        FooterActionsColumn.Width = compact ? new GridLength(0) : new GridLength(1, GridUnitType.Auto);

        Grid.SetRow(FooterButtons, compact ? 1 : 0);
        Grid.SetColumn(FooterButtons, compact ? 0 : 1);
        FooterButtons.HorizontalAlignment = compact ? HorizontalAlignment.Stretch : HorizontalAlignment.Right;
        FooterButtons.Orientation = veryNarrow ? Orientation.Vertical : Orientation.Horizontal;

        PrimaryButton.HorizontalAlignment = compact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
        BackButton.HorizontalAlignment = compact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
    }
}
