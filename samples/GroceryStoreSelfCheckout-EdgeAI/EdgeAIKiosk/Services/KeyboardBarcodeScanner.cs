using System;
using System.Threading.Tasks;
using EdgeAIKiosk.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace EdgeAIKiosk.Services;

public sealed class KeyboardBarcodeScanner : IBarcodeScanner
{
    public event Action<string>? BarcodeScanned;

    private TextBox? _inputBox;

    public KeyboardBarcodeScanner(TextBox inputBox)
    {
        _inputBox = inputBox;
    }

    public Task StartAsync()
    {
        if (_inputBox is not null)
        {
            _inputBox.KeyDown += OnKeyDown;
            _inputBox.Focus(FocusState.Programmatic);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (_inputBox is not null)
            _inputBox.KeyDown -= OnKeyDown;
        return Task.CompletedTask;
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && _inputBox is not null)
        {
            var barcode = _inputBox.Text.Trim();
            if (!string.IsNullOrEmpty(barcode))
                BarcodeScanned?.Invoke(barcode);
            _inputBox.Text = string.Empty;
        }
    }

    public void Dispose()
    {
        if (_inputBox is not null)
            _inputBox.KeyDown -= OnKeyDown;
    }
}
