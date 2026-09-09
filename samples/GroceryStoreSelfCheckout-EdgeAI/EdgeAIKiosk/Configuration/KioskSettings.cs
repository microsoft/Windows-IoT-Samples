using System;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime;

namespace EdgeAIKiosk;

public static class KioskSettings
{
    public static string ModelFileName { get; set; } = "Models\\yolo26x.onnx";
    public static string? CameraDeviceId { get; set; }
    // Null keeps automatic NPU, GPU, then CPU selection.
    public static OrtHardwareDeviceType? PreferredHardware { get; set; }
    public static string LabelTypeName { get; set; } = "CocoLabels";
    public static HashSet<string> AcceptedLabels { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "apple",
        "banana",
        "orange",
        "bottle",
        "cup"
    };
}
