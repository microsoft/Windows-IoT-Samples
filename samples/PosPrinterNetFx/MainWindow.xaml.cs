using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.PointOfService;

namespace PosPrinterNetFx
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PosExplorer _explorer;
        private readonly List<DeviceInfo> _printerDevices = new List<DeviceInfo>();
        private PosPrinter _printer;

        public MainWindow()
        {
            InitializeComponent();
            InitializePrinters();
            PrinterComboBox.SelectionChanged += PrinterComboBox_SelectionChanged;
            // Optional default sample text
            ReceiptTextBox.Text = "Sample receipt line 1\r\nSample receipt line 2\r\nThank you!";
        }

        private void InitializePrinters()
        {
            _explorer = new PosExplorer();
            foreach (DeviceInfo device in _explorer.GetDevices())
            {
                if (device.Type == DeviceType.PosPrinter)
                {
                    _printerDevices.Add(device);
                }
            }

            // Populate ComboBox
            PrinterComboBox.Items.Clear();
            foreach (var d in _printerDevices)
            {
                // Display ServiceObjectName (could also use Description)
                PrinterComboBox.Items.Add(d.ServiceObjectName);
            }

            if (_printerDevices.Count > 0)
            {
                int index = 0; // select first printer as default
                PrinterComboBox.SelectedIndex = index;
                CreatePrinterInstance(_printerDevices[index]);
            }
        }

        private void PrinterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PrinterComboBox.SelectedIndex < 0 || PrinterComboBox.SelectedIndex >= _printerDevices.Count)
                return;

            // Close previous instance if needed
            try
            {
                if (_printer != null && _printer.State != ControlState.Closed)
                {
                    if (_printer.Claimed)
                    {
                        _printer.Release();
                    }
                    _printer.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cleanup error in PrinterComboBox_SelectionChanged: {ex}");
            }

            CreatePrinterInstance(_printerDevices[PrinterComboBox.SelectedIndex]);
        }

        private void CreatePrinterInstance(DeviceInfo deviceInfo)
        {
            try
            {
                _printer = (PosPrinter)_explorer.CreateInstance(deviceInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create printer instance: {ex.Message}");
                _printer = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_printer == null)
            {
                MessageBox.Show("No POS printer selected.");
                return;
            }

            string textToPrint = ReceiptTextBox.Text;
            if (string.IsNullOrWhiteSpace(textToPrint))
            {
                textToPrint = "(No text entered)\r\n\r\n";
            }

            try
            {
                _printer.Open();
                _printer.Claim(1000);
                _printer.DeviceEnabled = true;
                _printer.PrintNormal(PrinterStation.Receipt, textToPrint + "\r\n\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print failed: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (_printer.Claimed)
                    {
                        _printer.Release();
                    }
                    _printer.Close();
                }
                catch { /* ignore close errors */ }
            }
        }
    }
}
