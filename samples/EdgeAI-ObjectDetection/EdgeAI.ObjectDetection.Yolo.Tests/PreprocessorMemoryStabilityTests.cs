using EdgeAI.ObjectDetection.Yolo.Pipeline;
using System.Diagnostics;
using Windows.Graphics.Imaging;

namespace EdgeAI.ObjectDetection.Yolo.Tests;

[Collection(MemoryStabilityCollection.Name)]
[Trait("Category", "MemoryStability")]
public class PreprocessorMemoryStabilityTests
{
    [MemoryStabilityFact]
    public void Preprocessor_DoesNotGrowMemoryOverRepeatedFrames()
    {
        YoloPreprocessor preprocessor = new();
        using SoftwareBitmap frame = new(BitmapPixelFormat.Bgra8, 640, 480, BitmapAlphaMode.Ignore);
        PreviewGeometry crop = new(0, 0, 640, 480, 1, 0, 0);

        for (int i = 0; i < 20; i++) preprocessor.Preprocess(frame, crop, false);
        long baseline = PrivateBytes();

        for (int i = 0; i < 1000; i++) preprocessor.Preprocess(frame, crop, false);

        Assert.True(PrivateBytes() <= baseline + (32L * 1024L * 1024L));
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
