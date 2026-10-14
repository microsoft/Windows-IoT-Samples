using EdgeAI.ObjectDetection.Yolo.Pipeline;
using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;
using Windows.Graphics.Imaging;

namespace EdgeAI.ObjectDetection.Yolo.Tests;

[Collection(MemoryStabilityCollection.Name)]
[Trait("Category", "MemoryStability")]
public class YoloInferenceMemoryStabilityTests
{
    [MemoryStabilityFact]
    public void YoloInference_DoesNotGrowMemoryOverRepeatedRuns()
    {
        string modelPath = Path.Combine(RepoRoot(), "EdgeAIKiosk", "Models", "yolo26x.onnx");
        using SoftwareBitmap frame = new(BitmapPixelFormat.Bgra8, 640, 640, BitmapAlphaMode.Ignore);
        PreprocessedFrame input = new YoloPreprocessor().Preprocess(frame, new PreviewGeometry(0, 0, 640, 640, 1, 0, 0), false);
        OrtEpDevice cpu = OrtEnv.Instance().GetEpDevices().First(device => device.HardwareDevice.Type == OrtHardwareDeviceType.CPU);
        using YoloInferenceEngine loader = YoloInferenceEngine.Create(modelPath, cpu);

        for (int i = 0; i < 10; i++) loader.Run(input);
        long baseline = PrivateBytes();

        for (int i = 0; i < 1000; i++) loader.Run(input);

        Assert.True(PrivateBytes() <= baseline + (64L * 1024L * 1024L));
    }

    private static string RepoRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "EdgeAIKiosk")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find repository root.");
    }

    private static long PrivateBytes()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        using Process process = Process.GetCurrentProcess();
        process.Refresh();
        return process.PrivateMemorySize64;
    }
}
