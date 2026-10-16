using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EdgeAI.SelfCheckoutKiosk.Views;

public sealed partial class ErrorOverlay : UserControl
{
    public event RoutedEventHandler? BackToHomeRequested;

    public ErrorOverlay()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (Visibility == Visibility.Visible)
            {
                BackToHomeButton.Focus(FocusState.Programmatic);
            }
        };
    }

    public void Show(string message)
    {
        ErrorMessageTextBlock.Text = message;
        Visibility = Visibility.Visible;
        BackToHomeButton.Focus(FocusState.Programmatic);
    }

    private void OnBackToHomeClick(object sender, RoutedEventArgs e) =>
        BackToHomeRequested?.Invoke(this, e);
}
