using EdgeAI_ObjectDetection.Controls;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;

namespace EdgeAIKiosk;

public enum ScannerMode
{
    HidScanner,
    Keyboard
}

public sealed class KioskSettings
{
    public string? ModelFileName { get; set; }
    public string? CameraGroupId { get; set; }
    // Null keeps automatic NPU, GPU, then CPU selection.
    public OrtHardwareDeviceType? PreferredHardware { get; set; }
    public ScannerMode ScannerMode { get; set; } = ScannerMode.HidScanner;
    public HashSet<string> AcceptedLabels { get; set; } =
        new(YoloInferenceView.GetSupportedClasses(), StringComparer.OrdinalIgnoreCase);
}
