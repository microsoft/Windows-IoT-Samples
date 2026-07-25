using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EdgeAIKiosk1;
using EdgeAIKiosk1.Models;
using EdgeAIKiosk1.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace EdgeAIKiosk1.Views;

public sealed partial class ShoppingView : Window
{
    // Cart state is displayed in the scanned-items list.
    public ObservableCollection<ScannedItem> ScannedItems { get; } = new();

    private readonly LiveInferenceView _liveInference = new();

    #region Startup

    public ShoppingView()
    {
        InitializeComponent();
        WindowLayout.Maximize(this);
        Activated += OnWindowActivated;
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs e) =>
        FocusBarcodeInput();

    private async void OnShoppingViewLoaded(object sender, RoutedEventArgs e)
    {
        FocusBarcodeInput();
        await StartupTask.Run(StartShopping, "Hardware connection error", ShowStartupError);
    }

    private void FocusBarcodeInput() =>
        DispatcherQueue.TryEnqueue(() => BarcodeInputBox.Focus(FocusState.Programmatic));

    /// <summary>
    /// Connects the cart and preview controls, starts inference, and removes the startup overlay.
    /// </summary>
    private async Task StartShopping()
    {
        ScannedItemsListBox.ItemsSource = ScannedItems;
        LiveInferenceHost.Content = _liveInference;
        await _liveInference.StartAsync();
        HideLoadingOverlay();
    }

    private void HideLoadingOverlay() =>
        LoadingOverlay.Visibility = Visibility.Collapsed;

    private void ShowStartupError(string message)
    {
        _ = _liveInference.StopAsync();
        ShowLoadingError(message);
    }

    #endregion

    #region Barcode scanning

    /// <summary>
    /// Accepts a unique barcode on Enter, syncs the cart with live inference, and resets scanner focus.
    /// </summary>
    private void OnBarcodeKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter) return;

        // Read and validate the scanner input.
        var barcode = BarcodeInputBox.Text.Trim();
        bool hasBarcode = !string.IsNullOrEmpty(barcode);
        bool alreadyScanned = ScannedItems.Any(item => item.Barcode == barcode);

        // Add new scans, then reset focus for the next scan.
        if (hasBarcode && !alreadyScanned)
            ScannedItems.Add(new ScannedItem(barcode, barcode, 1));
        _liveInference.SetScannedItems(ScannedItems);
        BarcodeInputBox.Text = string.Empty;
        FocusBarcodeInput();
    }

    #endregion

    #region Cart actions

    private void OnVoidItemClick(object sender, RoutedEventArgs e)
    {
        if (ScannedItemsListBox.SelectedItem is ScannedItem item)
            ScannedItems.Remove(item);
        _liveInference.SetScannedItems(ScannedItems);
        FocusBarcodeInput();
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
