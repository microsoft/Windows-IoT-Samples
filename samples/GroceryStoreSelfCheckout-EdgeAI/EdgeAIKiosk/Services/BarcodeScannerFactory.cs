using System.Threading.Tasks;
using EdgeAIKiosk.Interfaces;
using Microsoft.UI.Xaml;

namespace EdgeAIKiosk.Services;

public static class BarcodeScannerFactory
{
    public static async Task<IBarcodeScanner> CreateAsync(Window window)
    {
        // Try to find a connected HID barcode scanner.
        var device = await Windows.Devices.PointOfService.BarcodeScanner.GetDefaultAsync();
        if (device is not null)
        {
            device.Dispose();
            return new HidBarcodeScanner();
        }

        // No HID scanner found — fall back to keyboard mode.
        return new KeyboardBarcodeScanner(window);
    }
}
