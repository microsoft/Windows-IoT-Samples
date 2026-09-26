using System;
using System.Threading.Tasks;
using EdgeAIKiosk.Interfaces;
using Windows.Devices.PointOfService;

namespace EdgeAIKiosk.Services;

public sealed class HidBarcodeScanner : IBarcodeScanner
{
    public event Action<string>? BarcodeScanned;

    private BarcodeScanner? _device;
    private ClaimedBarcodeScanner? _claimedDevice;

    public async Task StartAsync()
    {
        _device = await BarcodeScanner.GetDefaultAsync();
        if (_device is null)
            throw new InvalidOperationException("No HID barcode scanner found.");

        _claimedDevice = await _device.ClaimScannerAsync();
        _claimedDevice.DataReceived += OnDataReceived;
        _claimedDevice.IsDecodeDataEnabled = true;
        await _claimedDevice.EnableAsync();
    }

    public async Task StopAsync()
    {
        if (_claimedDevice is not null)
        {
            await _claimedDevice.DisableAsync();
            _claimedDevice.DataReceived -= OnDataReceived;
            _claimedDevice.Dispose();
            _claimedDevice = null;
        }

        _device?.Dispose();
        _device = null;
    }

    private void OnDataReceived(
        ClaimedBarcodeScanner sender,
        BarcodeScannerDataReceivedEventArgs args)
    {
        var barcode = args.Report?.ScanDataLabel is { Length: > 0 } label
            ? System.Text.Encoding.UTF8.GetString(label.ToArray())
            : null;

        if (!string.IsNullOrEmpty(barcode))
            BarcodeScanned?.Invoke(barcode);
    }

    public void Dispose() => StopAsync().AsTask().GetAwaiter().GetResult();
}
