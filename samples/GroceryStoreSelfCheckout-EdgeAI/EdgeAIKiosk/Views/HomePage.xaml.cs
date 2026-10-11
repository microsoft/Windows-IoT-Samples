using EdgeAI_ObjectDetection.Controls;
using EdgeAI_ObjectDetection.Pipeline;
using EdgeAIKiosk.Services;
using Microsoft.ML.OnnxRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AI.MachineLearning;
using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;

namespace EdgeAIKiosk.Views;

public sealed partial class HomePage : Page
{
    private KioskSettings _settings = new();

    public HomePage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _settings = e.Parameter as KioskSettings ?? new KioskSettings();
        await StartupTask.Run(async () =>
        {
            if (_settings.ScannerMode == ScannerMode.Keyboard)
            {
                ScannerPickerComboBox.SelectedItem = KeyboardScannerItem;
            }
            else
            {
                ScannerPickerComboBox.SelectedItem = HidScannerItem;
            }
            LoadModelOptions();
            await LoadHardwareOptions();
            await LoadCameraOptions();
            if (ModelPickerComboBox.SelectedItem is null || CameraPickerComboBox.SelectedItem is null)
            {
                throw new InvalidOperationException("Select a compatible model and color camera before starting.");
            }
            StartNowButton.IsEnabled = true;
        }, "Startup setup failed", message =>
        {
            StartNowButton.IsEnabled = false;
            StartupErrorTextBlock.Text = message;
            StartupErrorTextBlock.Visibility = Visibility.Visible;
        });
    }

    private void LoadModelOptions()
    {
        List<string> modelNames = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Models"), "*.onnx")
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();
        ModelPickerComboBox.ItemsSource = modelNames;
        ModelPickerComboBox.SelectedItem = modelNames
            .FirstOrDefault(name => name == Path.GetFileName(_settings.ModelFileName))
            ?? modelNames.FirstOrDefault();
    }

    private async Task LoadHardwareOptions()
    {
        try
        {
            await ExecutionProviderCatalog.GetDefault().EnsureAndRegisterCertifiedAsync();
        }
        catch (COMException exception)
        {
            Trace.TraceWarning($"Windows ML provider registration failed: {exception.Message}");
        }

        List<HardwarePickerOption> options = BuildHardwareOptions(
            OrtEnv.Instance().GetEpDevices().Select(device => device.HardwareDevice.Type));
        HardwarePickerComboBox.ItemsSource = options;
        HardwarePickerComboBox.SelectedItem = options.FirstOrDefault(option => option.Value == _settings.PreferredHardware) ?? options[0];
    }

    internal static List<HardwarePickerOption> BuildHardwareOptions(IEnumerable<OrtHardwareDeviceType> availableHardware)
    {
        HashSet<OrtHardwareDeviceType> available = availableHardware.ToHashSet();
        IEnumerable<HardwarePickerOption> detected = new[] { OrtHardwareDeviceType.CPU, OrtHardwareDeviceType.GPU, OrtHardwareDeviceType.NPU }
            .Where(available.Contains)
            .Select(hardware => new HardwarePickerOption(hardware.ToString(), hardware));
        return [new("Auto", null), .. detected];
    }

    private async Task LoadCameraOptions()
    {
        IReadOnlyList<MediaFrameSourceGroup> groups = await MediaFrameSourceGroup.FindAllAsync();
        List<MediaFrameSourceGroup> cameras = groups
            .Where(group => group.SourceInfos.Any(YoloPipeline.IsColorVideoSource)).ToList();
        CameraPickerComboBox.ItemsSource = cameras;
        CameraPickerComboBox.SelectedItem = cameras.FirstOrDefault(group => group.Id == _settings.CameraGroupId)
            ?? cameras.FirstOrDefault();
    }

    /// <summary>
    /// Saves the selected model and camera and opens the shopping page.
    /// </summary>
    private void OnStartNowClick(object sender, RoutedEventArgs e)
    {
        if (ScannerPickerComboBox.SelectedItem is ComboBoxItem scanner && scanner == KeyboardScannerItem)
        {
            _settings.ScannerMode = ScannerMode.Keyboard;
        }
        else
        {
            _settings.ScannerMode = ScannerMode.HidScanner;
        }
        _settings.ModelFileName = Path.Combine("Models", (string)ModelPickerComboBox.SelectedItem);
        _settings.PreferredHardware = ((HardwarePickerOption)HardwarePickerComboBox.SelectedItem).Value;
        MediaFrameSourceGroup camera = (MediaFrameSourceGroup)CameraPickerComboBox.SelectedItem;
        _settings.CameraGroupId = camera.Id;
        KioskInferenceOptions options = new(Path.Combine(AppContext.BaseDirectory, _settings.ModelFileName),
            camera, _settings.PreferredHardware, _settings.AcceptedLabels.ToFrozenSet(StringComparer.OrdinalIgnoreCase));
        Frame.Navigate(typeof(ShoppingPage), new ShoppingNavigationArgs(_settings, options));
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        if (SettingsPanel.Visibility == Visibility.Visible)
        {
            HideSettings();
        }
        else
        {
            SettingsPanel.Visibility = Visibility.Visible;
            SettingsDismissLayer.Visibility = Visibility.Visible;
        }
    }

    private void OnSettingsDismissLayerTapped(object sender, TappedRoutedEventArgs e)
    {
        HideSettings();
    }

    private void HideSettings()
    {
        SettingsPanel.Visibility = Visibility.Collapsed;
        SettingsDismissLayer.Visibility = Visibility.Collapsed;
    }

    private void OnEditLabelsClick(object sender, RoutedEventArgs e)
    {
        IReadOnlyList<string> labels = YoloInferenceView.GetSupportedClasses();
        LabelsItemsView.ItemsSource = labels;
        LabelsItemsView.DeselectAll();
        for (int index = 0; index < labels.Count; index++)
        {
            if (_settings.AcceptedLabels.Contains(labels[index]))
            {
                LabelsItemsView.Select(index);
            }
        }
        LabelDialogOverlay.Visibility = Visibility.Visible;
    }

    private void OnCancelLabelsClick(object sender, RoutedEventArgs e)
    {
        LabelDialogOverlay.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Persists the selected labels, then closes the dialog.
    /// </summary>
    private void OnSaveLabelsClick(object sender, RoutedEventArgs e)
    {
        _settings.AcceptedLabels = new HashSet<string>(
            LabelsItemsView.SelectedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);
        LabelDialogOverlay.Visibility = Visibility.Collapsed;
    }

    internal sealed record HardwarePickerOption(string Name, OrtHardwareDeviceType? Value);
}
