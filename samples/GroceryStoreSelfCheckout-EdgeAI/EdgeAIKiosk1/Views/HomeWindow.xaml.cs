using EdgeAIKiosk1;
using EdgeAIKiosk1.Services;
using Microsoft.ML.OnnxRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AI.MachineLearning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using Windows.Media.Devices;

namespace EdgeAIKiosk1.Views;

public sealed partial class HomeWindow : Window
{
    public HomeWindow()
    {
        InitializeComponent();
        WindowLayout.Maximize(this);
    }

    private async void OnHomeWindowLoaded(object sender, RoutedEventArgs e) =>
        await StartupTask.Run(LoadPickerOptions, "Startup setup failed", ShowStartupError);

    #region Picker setup

    private async Task LoadPickerOptions()
    {
        LoadModelOptions();
        await LoadHardwareOptions();
        await LoadCameraOptions();
        StartNowButton.IsEnabled = true;
    }

    private void LoadModelOptions()
    {
        var modelOptions = Directory.GetFiles(Path.Combine(System.AppContext.BaseDirectory, "Models"), "*.onnx")
            .Select(path => new PickerOption(Path.GetFileName(path), Path.Combine("Models", Path.GetFileName(path))))
            .ToList();
        ModelPickerComboBox.ItemsSource = modelOptions;
        ModelPickerComboBox.SelectedItem = FindSelectedOption(modelOptions, KioskSettings.ModelFileName);
    }

    private static PickerOption? FindSelectedOption(IReadOnlyList<PickerOption> options, string? selectedValue) =>
        options.FirstOrDefault(option => option.Value == selectedValue) ?? options.FirstOrDefault();

    private async Task LoadHardwareOptions()
    {
        try { await ExecutionProviderCatalog.GetDefault().EnsureAndRegisterCertifiedAsync(); }
        catch (COMException exception) { Trace.TraceWarning($"Windows ML provider registration failed: {exception.Message}"); }
        var options = BuildHardwareOptions(OrtEnv.Instance().GetEpDevices().Select(device => device.HardwareDevice.Type));
        HardwarePickerComboBox.ItemsSource = options;
        HardwarePickerComboBox.SelectedItem = options.FirstOrDefault(option => option.Value == KioskSettings.PreferredHardware) ?? options[0];
    }

    internal static List<HardwarePickerOption> BuildHardwareOptions(IEnumerable<OrtHardwareDeviceType> availableHardware)
    {
        var available = availableHardware.ToHashSet();
        var detected = new[] { OrtHardwareDeviceType.CPU, OrtHardwareDeviceType.GPU, OrtHardwareDeviceType.NPU }
            .Where(available.Contains)
            .Select(hardware => new HardwarePickerOption(hardware.ToString(), hardware));
        return [new("Auto", null), .. detected];
    }

    private async Task LoadCameraOptions()
    {
        var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(MediaDevice.GetVideoCaptureSelector());
        var cameraOptions = devices.Select(device => new PickerOption(device.Name, device.Id)).ToList();
        CameraPickerComboBox.ItemsSource = cameraOptions;
        CameraPickerComboBox.SelectedItem = FindSelectedOption(cameraOptions, KioskSettings.CameraDeviceId) ?? cameraOptions.FirstOrDefault();
    }

    private void ShowStartupError(string message)
    {
        StartNowButton.IsEnabled = false;
        StartupErrorTextBlock.Text = message;
        StartupErrorTextBlock.Visibility = Visibility.Visible;
    }

    #endregion

    #region Navigation

    /// <summary>
    /// Saves the selected model and camera, opens the shopping flow, and closes this window.
    /// </summary>
    private void OnStartNowClick(object sender, RoutedEventArgs e)
    {
        KioskSettings.ModelFileName = ((PickerOption)ModelPickerComboBox.SelectedItem).Value;
        KioskSettings.PreferredHardware = ((HardwarePickerOption)HardwarePickerComboBox.SelectedItem).Value;
        KioskSettings.CameraDeviceId = ((PickerOption?)CameraPickerComboBox.SelectedItem)?.Value;
        new ShoppingView().Activate();
        Close();
    }

    #endregion

    #region Settings

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        if (SettingsPanel.Visibility == Visibility.Visible) HideSettings();
        else ShowSettings();
    }

    private void OnSettingsDismissLayerTapped(object sender, TappedRoutedEventArgs e) =>
        HideSettings();

    private void ShowSettings()
    {
        SettingsPanel.Visibility = Visibility.Visible;
        SettingsDismissLayer.Visibility = Visibility.Visible;
    }

    private void HideSettings()
    {
        SettingsPanel.Visibility = Visibility.Collapsed;
        SettingsDismissLayer.Visibility = Visibility.Collapsed;
    }

    #endregion

    #region Label dialog

    private void OnEditLabelsClick(object sender, RoutedEventArgs e)
    {
        PopulateLabelTypeOptions();
        PopulateLabelCheckboxes();
        LabelDialogOverlay.Visibility = Visibility.Visible;
    }

    private void PopulateLabelTypeOptions()
    {
        var labelTypeOptions = new List<PickerOption> { new("CocoLabels", "CocoLabels") };
        LabelTypeComboBox.ItemsSource = labelTypeOptions;
        LabelTypeComboBox.SelectedItem = FindSelectedOption(labelTypeOptions, KioskSettings.LabelTypeName);
    }

    private void PopulateLabelCheckboxes()
    {
        LabelsStackPanel.Children.Clear();
        foreach (var labelRow in CocoLabels.Labels.Chunk(3))
        {
            LabelsStackPanel.Children.Add(BuildLabelRow(labelRow));
        }
    }

    /// <summary>
    /// Builds one three-column row of label filters for the settings dialog.
    /// </summary>
    private StackPanel BuildLabelRow(IEnumerable<string> labels)
    {
            var row = new StackPanel();
        row.Orientation = Orientation.Horizontal;
        row.Spacing = 8;
        foreach (var label in labels) row.Children.Add(BuildLabelCheckBox(label));
        return row;
    }

    private CheckBox BuildLabelCheckBox(string label)
    {
        var checkBox = new CheckBox();
        checkBox.Content = label;
        checkBox.Tag = label;
        checkBox.Width = 180;
        checkBox.IsChecked = KioskSettings.AcceptedLabels.Contains(label);
        checkBox.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
        return checkBox;
    }

    private void OnCancelLabelsClick(object sender, RoutedEventArgs e) =>
        LabelDialogOverlay.Visibility = Visibility.Collapsed;

    /// <summary>
    /// Persists the selected label type and checked labels, then closes the dialog.
    /// </summary>
    private void OnSaveLabelsClick(object sender, RoutedEventArgs e)
    {
        KioskSettings.LabelTypeName = ((PickerOption)LabelTypeComboBox.SelectedItem).Value;
        var selectedLabels = LabelsStackPanel.Children.OfType<StackPanel>().SelectMany(LabelCheckboxesInRow).Where(IsChecked).Select(LabelFromCheckBox);
        KioskSettings.AcceptedLabels = new HashSet<string>(selectedLabels, StringComparer.OrdinalIgnoreCase);
        LabelDialogOverlay.Visibility = Visibility.Collapsed;
    }

    private static IEnumerable<CheckBox> LabelCheckboxesInRow(StackPanel row) =>
        row.Children.OfType<CheckBox>();

    private static bool IsChecked(CheckBox checkBox) =>
        checkBox.IsChecked == true;

    private static string LabelFromCheckBox(CheckBox checkBox) =>
        (string)checkBox.Tag;

    #endregion

    private sealed record PickerOption(string Name, string Value);
    internal sealed record HardwarePickerOption(string Name, OrtHardwareDeviceType? Value);
}
