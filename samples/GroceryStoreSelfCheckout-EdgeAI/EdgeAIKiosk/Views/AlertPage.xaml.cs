using System;
using System.Threading.Tasks;
using EdgeAI.ObjectDetection.Yolo;
using EdgeAIKiosk.Models;
using EdgeAIKiosk.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EdgeAIKiosk.Views;

internal sealed record CheckoutNavigationArgs(VerificationResult Result, YoloInferenceView Inference, KioskSettings Settings);

public sealed partial class AlertPage : Page
{
    private CheckoutNavigationArgs? _checkout;
    private Task _startupTask = Task.CompletedTask;
    private bool _returningHome;

    public AlertPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _startupTask = StartupTask.Run(async () =>
        {
            if (e.Parameter is not CheckoutNavigationArgs checkout)
            {
                throw new ArgumentException("Checkout requires a verification result and its inference session.", nameof(e));
            }

            _checkout = checkout;
            checkout.Inference.Faulted += OnInferenceFaulted;
            SuccessPanel.Visibility = checkout.Result.IsMatch ? Visibility.Visible : Visibility.Collapsed;
            FailurePanel.Visibility = checkout.Result.IsMatch ? Visibility.Collapsed : Visibility.Visible;
            MismatchedItemsListView.ItemsSource = checkout.Result.Mismatches;

            if (checkout.Result.IsMatch)
            {
                await checkout.Inference.DisposeAsync();
            }
            else
            {
                LiveInferenceHost.Content = checkout.Inference;
            }
        }, "Verification preview failed", ShowError);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (_checkout is not null)
        {
            _checkout.Inference.Faulted -= OnInferenceFaulted;
        }
    }

    private void OnInferenceFaulted(object? sender, InferenceFaultedEventArgs args) =>
        ShowError($"Inference failed: {args.Exception.Message}");

    private void ShowError(string message)
    {
        SuccessPanel.Visibility = Visibility.Collapsed;
        FailurePanel.Visibility = Visibility.Visible;
        MismatchedItemsListView.ItemsSource = new[] { message };
    }

    /// <summary>
    /// Stops the shared inference session before returning home.
    /// </summary>
    private async void OnStartAgainClick(object sender, RoutedEventArgs e)
    {
        if (_returningHome)
        {
            return;
        }
        _returningHome = true;
        await StartupTask.Run(async () =>
        {
            await _startupTask;
            if (_checkout is not null)
            {
                await _checkout.Inference.DisposeAsync();
            }
            LiveInferenceHost.Content = null;
            if (!Frame.Navigate(typeof(HomePage), _checkout?.Settings))
            {
                throw new InvalidOperationException("Could not navigate home.");
            }
        }, "Could not stop inference or return home", message =>
        {
            _returningHome = false;
            ShowError(message);
        });
    }
}
