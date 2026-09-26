using System;
using System.Threading.Tasks;
using EdgeAIKiosk.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace EdgeAIKiosk.Services;

public sealed class KeyboardBarcodeScanner : IBarcodeScanner
{
    public event Action<string>? BarcodeScanned;

    private readonly Window _window;
    private string _buffer = string.Empty;

    public KeyboardBarcodeScanner(Window window)
    {
        _window = window;
    }

    public Task StartAsync()
    {
        _window.Content.KeyDown += OnKeyDown;
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _window.Content.KeyDown -= OnKeyDown;
        return Task.CompletedTask;
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            var barcode = _buffer.Trim();
            if (!string.IsNullOrEmpty(barcode))
                BarcodeScanned?.Invoke(barcode);
            _buffer = string.Empty;
        }
        else
        {
            _buffer += InputKeyHelper.ToChar(e.Key);
        }
    }

    public void Dispose() => _ = StopAsync();
}

internal static class InputKeyHelper
{
    public static string ToChar(VirtualKey key) =>
        key >= VirtualKey.Number0 && key <= VirtualKey.Number9
            ? ((char)('0' + key - VirtualKey.Number0)).ToString()
            : key >= VirtualKey.A && key <= VirtualKey.Z
                ? ((char)('A' + key - VirtualKey.A)).ToString()
                : key.ToString();
}
