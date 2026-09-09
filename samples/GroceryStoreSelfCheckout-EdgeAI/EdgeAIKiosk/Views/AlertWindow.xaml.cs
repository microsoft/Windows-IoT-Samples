using System.Threading.Tasks;
using EdgeAIKiosk.Models;
using EdgeAIKiosk.Services;
using Microsoft.UI.Xaml;

namespace EdgeAIKiosk.Views;

public sealed partial class AlertWindow : Window
{
    private readonly VerificationResult _verificationResult;
    private readonly LiveInferenceView _liveInference;

    public AlertWindow(VerificationResult result, LiveInferenceView liveInference)
    {
        InitializeComponent();
        WindowLayout.Maximize(this);
        _verificationResult = result;
        _liveInference = liveInference;
        _liveInference.SetScannedItems(result.ScannedItems);
    }

    private async void OnAlertWindowLoaded(object sender, RoutedEventArgs e) =>
        await StartupTask.Run(ShowResult, "Verification preview failed", ShowStartupError);

    /// <summary>
    /// Shows success and releases inference resources, or shows mismatches and resumes the shared preview.
    /// </summary>
    private async Task ShowResult()
    {
        if (_verificationResult.IsMatch)
        {
            SuccessPanel.Visibility = Visibility.Visible;
            FailurePanel.Visibility = Visibility.Collapsed;
            await _liveInference.StopAsync();
            return;
        }

        FailurePanel.Visibility = Visibility.Visible;
        SuccessPanel.Visibility = Visibility.Collapsed;
        MismatchedItemsListView.ItemsSource = _verificationResult.Mismatches;
        LiveInferenceHost.Content = _liveInference;
        _liveInference.Resume();
    }

    /// <summary>
    /// Stops the shared inference session before returning to a fresh home window.
    /// </summary>
    private async void OnStartAgainClick(object sender, RoutedEventArgs e)
    {
        await _liveInference.StopAsync();
        new HomeWindow().Activate();
        Close();
    }

    private void ShowStartupError(string message)
    {
        SuccessPanel.Visibility = Visibility.Collapsed;
        FailurePanel.Visibility = Visibility.Visible;
        MismatchedItemsListView.ItemsSource = new[] { message };
    }
}
