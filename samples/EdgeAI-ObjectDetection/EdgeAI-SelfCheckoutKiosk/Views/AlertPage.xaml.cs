using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EdgeAI.ObjectDetection.Yolo;
using EdgeAI.SelfCheckoutKiosk.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EdgeAI.SelfCheckoutKiosk.Views;

internal sealed record CheckoutNavigationArgs(VerificationResult Result, YoloInferenceView Inference, KioskSettings Settings);

public sealed partial class AlertPage : Page
{
    private CheckoutNavigationArgs? _checkout;
    private Task _cleanupTask = Task.CompletedTask;
    private bool _returningHome;

    public AlertPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try
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

            if (!checkout.Result.IsMatch)
            {
                LiveInferenceHost.Content = checkout.Inference;
            }
        }
        catch (Exception error)
        {
            Trace.TraceError(error.ToString());
            ShowError($"Verification preview failed: {error.Message}");
            return;
        }

        if (_checkout.Result.IsMatch)
        {
            _cleanupTask = _checkout.Inference.DisposeAsync().AsTask();
            await _cleanupTask;
        }
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
        FailurePanel.Visibility = Visibility.Collapsed;
        ErrorView.Show(message);
    }

    /// <summary>
    /// Stops the shared inference session before returning home.
    /// </summary>
    private async void OnBackToHomeRequested(object sender, RoutedEventArgs e)
    {
        if (_returningHome)
        {
            return;
        }
        _returningHome = true;
        ErrorView.IsEnabled = false;
        await _cleanupTask;
        if (_checkout is not null)
        {
            await _checkout.Inference.DisposeAsync();
        }
        try
        {
            LiveInferenceHost.Content = null;
            if (!Frame.Navigate(typeof(HomePage), _checkout?.Settings))
            {
                throw new InvalidOperationException("Could not navigate home.");
            }
        }
        catch (Exception error)
        {
            Trace.TraceError(error.ToString());
            _returningHome = false;
            ErrorView.IsEnabled = true;
            ShowError($"Could not return home: {error.Message}");
        }
    }
}
