using EdgeAI.ObjectDetection.Yolo;
using EdgeAIKiosk.Interfaces;
using EdgeAIKiosk.Models;
using EdgeAIKiosk.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace EdgeAIKiosk.Views;

internal sealed record ShoppingNavigationArgs(KioskSettings Settings, KioskInferenceOptions Options);

public sealed partial class ShoppingPage : Page
{
    public ObservableCollection<ScannedItem> ScannedItems { get; } = new();
    public Visibility BarcodeInputVisibility =>
        _settings.ScannerMode == ScannerMode.Keyboard ? Visibility.Visible : Visibility.Collapsed;

    private KioskSettings _settings = new();
    private KioskInferenceOptions? _options;
    private readonly YoloInferenceView _inferenceView = new()
    {
        PreviewStretch = Stretch.UniformToFill,
        Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
        ShowConfidence = false,
        ShowInferenceTime = false,
        ShowEndToEndInferenceTime = false
    };
    private IBarcodeScanner? _scanner;
    private IReadOnlyList<PreviewDetection>? _latestDetections;
    private bool _cartEnabled;
    private bool _returningHome;

    public ShoppingPage()
    {
        InitializeComponent();
        ScannedItemsListBox.ItemsSource = ScannedItems;
        LiveInferenceHost.Content = _inferenceView;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _inferenceView.Faulted -= OnInferenceFaulted;
        _inferenceView.DetectionsUpdated -= OnDetectionsUpdated;
        DisposeScanner();
    }

    private void DisposeScanner()
    {
        if (_scanner is not null)
        {
            _scanner.BarcodeScanned -= OnBarcodeScanned;
        }
        _scanner?.Dispose();
        _scanner = null;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        SetCartEnabled(false);
        try
        {
            if (e.Parameter is not ShoppingNavigationArgs navigation)
            {
                throw new ArgumentException("Shopping requires settings and inference options.", nameof(e));
            }
            _settings = navigation.Settings;
            _options = navigation.Options;
            Bindings.Update();
            _inferenceView.Faulted += OnInferenceFaulted;
            _inferenceView.DetectionsUpdated += OnDetectionsUpdated;
            _scanner = _settings.ScannerMode == ScannerMode.HidScanner
                ? new HidBarcodeScanner()
                : new KeyboardBarcodeScanner(BarcodeInputBox);
            _scanner.BarcodeScanned += OnBarcodeScanned;
            await _scanner.StartAsync();
            await KioskInference.StartAsync(_inferenceView, _options, CancellationToken.None);
            LoadingOverlay.Visibility = Visibility.Collapsed;
            SetCartEnabled(true);
            if (_settings.ScannerMode == ScannerMode.Keyboard)
            {
                BarcodeInputBox.Focus(FocusState.Programmatic);
            }
        }
        catch (Exception error)
        {
            Trace.TraceError(error.ToString());
            DisposeScanner();
            await _inferenceView.DisposeAsync();
            ShowError($"Hardware connection error: {error.Message}");
        }
    }

    private void OnInferenceFaulted(object? sender, InferenceFaultedEventArgs args)
    {
        ShowError($"Inference failed: {args.Exception.Message}");
    }

    private void OnDetectionsUpdated(object? sender, DetectionsUpdatedEventArgs args)
    {
        _latestDetections = args.Detections;
        DetectedCountTextBlock.Text = $"Detected: {args.Detections.Count} items";
        PayNowButton.IsEnabled = _cartEnabled;
    }

    private void SetCartEnabled(bool enabled)
    {
        _cartEnabled = enabled;
        PayNowButton.IsEnabled = enabled && _latestDetections is not null;
        VoidItemButton.IsEnabled = enabled;
        BarcodeInputBox.IsEnabled = enabled;
    }

    private void OnBarcodeScanned(string barcode)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (!_cartEnabled || ScannedItems.Any(item => item.Barcode == barcode))
            {
                return;
            }
            ScannedItems.Add(new ScannedItem(barcode, barcode, 1));
        });
    }

    private void OnVoidItemClick(object sender, RoutedEventArgs e)
    {
        if (ScannedItemsListBox.SelectedItem is ScannedItem item)
        {
            ScannedItems.Remove(item);
        }
    }

    /// <summary>
    /// Compares the cart with the latest completed streaming result and opens the result page.
    /// </summary>
    private void OnPayNowClick(object sender, RoutedEventArgs e)
    {
        SetCartEnabled(false);
        LoadingProgressRing.Visibility = Visibility.Visible;
        LoadingTextBlock.Text = "Verifying basket...";
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            if (_latestDetections is null || _options is null)
            {
                throw new InvalidOperationException("Wait for the first inference result before checking out.");
            }
            VerificationResult result = ShoppingVerifier.Verify(ScannedItems, _latestDetections, _options.SelectedClasses);
            LiveInferenceHost.Content = null;
            if (!Frame.Navigate(typeof(AlertPage), new CheckoutNavigationArgs(result, _inferenceView, _settings)))
            {
                throw new InvalidOperationException("Could not navigate to the checkout result.");
            }
        }
        catch (Exception error)
        {
            Trace.TraceError(error.ToString());
            LiveInferenceHost.Content = _inferenceView;
            ShowError($"Verification failed: {error.Message}");
        }
    }

    private void ShowError(string message)
    {
        SetCartEnabled(false);
        LoadingOverlay.Visibility = Visibility.Collapsed;
        ErrorView.Show(message);
    }

    private async void OnBackToHomeRequested(object sender, RoutedEventArgs e)
    {
        if (_returningHome)
        {
            return;
        }
        _returningHome = true;
        ErrorView.IsEnabled = false;
        DisposeScanner();
        await _inferenceView.DisposeAsync();
        try
        {
            LiveInferenceHost.Content = null;
            if (!Frame.Navigate(typeof(HomePage), _settings))
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
