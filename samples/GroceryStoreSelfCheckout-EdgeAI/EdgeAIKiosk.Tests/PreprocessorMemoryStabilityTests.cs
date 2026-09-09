using System.Diagnostics;
using EdgeAIKiosk.Pipeline;
using Windows.Graphics.Imaging;

namespace EdgeAIKiosk.Tests;

public class PreprocessorMemoryStabilityTests
{
    private const string EnableVariable = "RUN_MEMORY_STABILITY_TESTS";

    [Fact]
    public void Preprocessor_DoesNotGrowMemoryOverRepeatedFrames()
    {
        if (Environment.GetEnvironmentVariable(EnableVariable) != "1") return;

        var preprocessor = new Yolo26Preprocessor();
        using var frame = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 640, 480, BitmapAlphaMode.Ignore);

        for (var i = 0; i < 20; i++) preprocessor.Preprocess(frame);
        var baseline = PrivateBytes();

        for (var i = 0; i < 1000; i++) preprocessor.Preprocess(frame);

        Assert.True(PrivateBytes() <= baseline + (32L * 1024L * 1024L));
    }

    private static long PrivateBytes()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        using var process = Process.GetCurrentProcess();
        process.Refresh();
        return process.PrivateMemorySize64;
    }
}
