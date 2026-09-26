using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EdgeAIKiosk;
using EdgeAIKiosk.Models;
using EdgeAIKiosk.Services;
using EdgeAIKiosk.Interfaces;
using Microsoft.UI.Xaml;

namespace EdgeAIKiosk.Views;

public sealed partial class ShoppingView : Window
{
    // Cart state is displayed in the scanned-items list.
    public ObservableCollection<ScannedItem> ScannedItems { get; } = new();

    private readonly LiveInferenceView _liveInference = new();
    private IBarcodeScanner? _scanner;

    #region Startup

    public ShoppingView()
    {
        InitializeComponent();
        WindowLayout.Maximize(this);
    }

    private async void OnShoppingViewLoaded(object sender, RoutedEventArgs e)
    {
        await StartupTask.Run(StartShopping, "Hardware connection error", ShowStartupError);
    }

    /// <summary>
    /// Connects the cart and preview controls, starts inference, and removes the startup overlay.
    /// </summary>
    private async Task StartShopping()
    {
        ScannedItemsListBox.ItemsSource = ScannedItems;
        LiveInferenceHost.Content = _liveInference;
        _scanner = await BarcodeScannerFactory.CreateAsync(this);
        _scanner.BarcodeScanned += OnBarcodeScanned;
        await _scanner.StartAsync();
        await _liveInference.StartAsync();
        HideLoadingOverlay();
    }

    private void HideLoadingOverlay() =>
        LoadingOverlay.Visibility = Visibility.Collapsed;

    private void ShowStartupError(string message)
    {
        _scanner?.Dispose();
        _ = _liveInference.StopAsync();
        ShowLoadingError(message);
    }

    #endregion

    #region Barcode scanning

    /// <summary>
    /// Receives a barcode from any scanner type, adds unique items to the cart, and syncs with live inference.
    /// </summary>
    private void OnBarcodeScanned(string barcode)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (ScannedItems.Any(item => item.Barcode == barcode)) return;
            ScannedItems.Add(new ScannedItem(barcode, barcode, 1));
            _liveInference.SetScannedItems(ScannedItems);
        });
    }

    #endregion

    #region Cart actions

    private void OnVoidItemClick(object sender, RoutedEventArgs e)
    {
        if (ScannedItemsListBox.SelectedItem is ScannedItem item)
            ScannedItems.Remove(item);
        _liveInference.SetScannedItems(ScannedItems);
    }

    #endregion

    #region Checkout

    /// <summary>
    /// Pauses live inference, verifies the scanned cart against a captured frame, and opens the result window.
    /// </summary>
    private async void OnPayNowClick(object sender, RoutedEventArgs e)
    {
        // Verify the cart contents against camera detections.
        PayNowButton.IsEnabled = false;
        ShowLoadingOverlay("Verifying basket...");
        try
        {
            await _liveInference.PauseAsync();
            var shoppingVerifier = VerifierFactory.Create(
                _liveInference.ImageCapture,
                _liveInference.Preprocessor,
                _liveInference.ModelLoader);
            var verificationResult = await shoppingVerifier.Verify(ScannedItems.ToList());

            LiveInferenceHost.Content = null;
            new AlertWindow(verificationResult, _liveInference).Activate();
            Close();
        }
        catch (Exception exception)
        {
            Trace.TraceError(exception.ToString());
            _liveInference.Resume();
            PayNowButton.IsEnabled = true;
            ShowLoadingError($"Verification failed: {exception.Message}");
        }
    }

    #endregion

    #region Loading overlay

    private void ShowLoadingOverlay(string message)
    {
        LoadingProgressRing.Visibility = Visibility.Visible;
        LoadingTextBlock.Text = message;
        LoadingOverlay.Visibility = Visibility.Visible;
    }

    private void ShowLoadingError(string message)
    {
        LoadingProgressRing.Visibility = Visibility.Collapsed;
        LoadingTextBlock.Text = message;
        LoadingOverlay.Visibility = Visibility.Visible;
    }

    #endregion
}
