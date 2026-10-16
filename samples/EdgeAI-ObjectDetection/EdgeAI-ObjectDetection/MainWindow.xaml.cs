// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using EdgeAI.ObjectDetection.Yolo;
using Microsoft.ML.OnnxRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AI.MachineLearning;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;
using Windows.Storage;

namespace EdgeAI.ObjectDetection;

public sealed partial class MainWindow : Window
{
    public ObservableCollection<StorageFile> Models
    {
        get;
    } = new();
    public ObservableCollection<MediaFrameSourceGroup> Cameras
    {
        get;
    } = new();
    private readonly SemaphoreSlim _selectionGate = new(1, 1);
    private bool _changingModels;

    public MainWindow()
    {
        InitializeComponent();
        UpdateControls(false);
        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        double scale = GetDpiForWindow(hwnd) / 96.0;
        AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(1200 * scale), (int)(800 * scale)));
        InferenceView.Faulted += (_, args) =>
        {
            HostStatusTextBlock.Text = args.Exception.Message;
            UpdateControls(true);
        };
        InferenceView.DetectionsUpdated += (_, args) =>
            HostStatusTextBlock.Text = $"{args.Detections.Count} detections";
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    private async void MainGrid_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            try
            {
                await ExecutionProviderCatalog.GetDefault().EnsureAndRegisterCertifiedAsync();
            }
            catch (COMException error)
            {
                Trace.TraceWarning(error.ToString());
                HostStatusTextBlock.Text = $"Provider registration warning: {error.Message}";
            }
            foreach (OrtEpDevice device in OrtEnv.Instance().GetEpDevices())
            {
                ExecutionProviderComboBox.Items.Add(new ComboBoxItem
                {
                    Content = $"{device.EpName} ({device.HardwareDevice.Type})",
                    Tag = device
                });
            }

            ExecutionProviderComboBox.SelectedIndex = ExecutionProviderComboBox.Items.Count > 0 ? 0 : -1;

            var cameras = await MediaFrameSourceGroup.FindAllAsync();
            foreach (var camera in cameras.Where(YoloInferenceView.IsSupportedCamera))
            {
                Cameras.Add(camera);
            }

            CameraComboBox.SelectedIndex = Cameras.Count > 0 ? 0 : -1;
            ModelComboBox.SelectionChanged += Settings_SelectionChanged;
            ExecutionProviderComboBox.SelectionChanged += Settings_SelectionChanged;
            CameraComboBox.SelectionChanged += Settings_SelectionChanged;
            InferenceTypeComboBox.SelectionChanged += Settings_SelectionChanged;
            HostStatusTextBlock.Text = Cameras.Count == 0
                ? "No compatible color camera was found."
                : "Choose a model folder to start.";
            UpdateControls(true);
        }
        catch (Exception error)
        {
            ReportError(error);
        }
    }

    private async void ModelFolderButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateControls(false);
        try
        {
            var result = await new FolderPicker(AppWindow.Id).PickSingleFolderAsync();
            if (result is null)
            {
                return;
            }

            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(result.Path);
            var files = await folder.GetFilesAsync();
            _changingModels = true;
            Models.Clear();
            foreach (StorageFile file in files
                .Where(file => file.FileType.Equals(".onnx", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase))
            {
                Models.Add(file);
            }

            ModelFolderStatusTextBlock.Text = Models.Count == 0
                ? $"No .onnx files found in {result.Path}" : result.Path;
            ModelComboBox.SelectedIndex = Models.Count > 0 ? 0 : -1;
            _changingModels = false;
            await RestartInferenceAsync();
        }
        catch (Exception error)
        {
            ReportError(error);
        }
        finally
        {
            _changingModels = false;
            UpdateControls(true);
        }
    }

    private async void Settings_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_changingModels)
        {
            return;
        }

        await RestartInferenceAsync();
    }

    private async Task RestartInferenceAsync()
    {
        await _selectionGate.WaitAsync();
        try
        {
            UpdateControls(false);
            await InferenceView.StopAsync();
            if (ModelComboBox.SelectedItem is not StorageFile model ||
                CameraComboBox.SelectedItem is not MediaFrameSourceGroup camera ||
                ExecutionProviderComboBox.SelectedItem is not ComboBoxItem { Tag: OrtEpDevice device })
            {
                HostStatusTextBlock.Text = "Select a model, camera, and execution provider.";
                return;
            }
            HostStatusTextBlock.Text = "Starting...";
            await InferenceView.StartAsync(new InferenceSettings
            {
                ModelPath = model.Path,
                FrameSourceGroup = camera,
                ExecutionProvider = device,
                InferenceType = InferenceTypeComboBox.SelectedIndex == 0 ? InferenceType.Stream : InferenceType.OneShot
            });
            HostStatusTextBlock.Text = "Ready.";
        }
        catch (Exception error)
        {
            ReportError(error);
        }
        finally
        {
            UpdateControls(true);
            _selectionGate.Release();
        }
    }

    private async void InferOnceButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateControls(false);
        try
        {
            using var snapshot = await InferenceView.InferOnceAsync();
            HostStatusTextBlock.Text = $"{snapshot.Detections.Count} detections";
        }
        catch (Exception error)
        {
            ReportError(error);
        }
        finally
        {
            UpdateControls(true);
        }
    }

    private void UpdateControls(bool enabled)
    {
        ModelFolderButton.IsEnabled = enabled;
        ModelComboBox.IsEnabled = enabled && Models.Count > 0;
        CameraComboBox.IsEnabled = enabled && Cameras.Count > 0;
        ExecutionProviderComboBox.IsEnabled = enabled && ExecutionProviderComboBox.Items.Count > 0;
        InferenceTypeComboBox.IsEnabled = enabled;
        InferOnceButton.IsEnabled = enabled && InferenceTypeComboBox.SelectedIndex == 1 &&
            InferenceView.State == YoloInferenceState.Running;
    }

    private void ReportError(Exception error)
    {
        Trace.TraceError(error.ToString());
        HostStatusTextBlock.Text = error.Message;
    }
}
