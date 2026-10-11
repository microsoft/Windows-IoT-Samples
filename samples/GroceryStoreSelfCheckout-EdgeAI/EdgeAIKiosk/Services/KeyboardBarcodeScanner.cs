using EdgeAIKiosk.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace EdgeAIKiosk.Services;

public sealed class KeyboardBarcodeScanner : IBarcodeScanner
{
    public event Action<string>? BarcodeScanned;

    private readonly TextBox _inputBox;

    public KeyboardBarcodeScanner(TextBox inputBox)
    {
        _inputBox = inputBox;
    }

    public Task StartAsync()
    {
        _inputBox.KeyDown += OnKeyDown;
        return Task.CompletedTask;
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            string barcode = _inputBox.Text.Trim();
            if (!string.IsNullOrEmpty(barcode))
            {
                BarcodeScanned?.Invoke(barcode);
            }
            _inputBox.Text = string.Empty;
        }
    }

    public void Dispose()
    {
        _inputBox.KeyDown -= OnKeyDown;
    }
}
