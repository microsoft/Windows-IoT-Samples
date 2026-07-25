using System.Diagnostics;
using EdgeAIKiosk1.Models;
using EdgeAIKiosk1.Pipeline;

namespace EdgeAIKiosk1.Tests;

public class YoloInferenceMemoryStabilityTests
{
    private const string EnableVariable = "RUN_MEMORY_STABILITY_TESTS";

    [Fact]
    public async Task YoloInference_DoesNotGrowMemoryOverRepeatedRuns()
    {
        if (Environment.GetEnvironmentVariable(EnableVariable) != "1") return;

        var modelPath = Path.Combine(RepoRoot(), "EdgeAIKiosk1", "Models", "yolo26x.onnx");
        var input = new ModelInput(new float[3 * 640 * 640], 640, 640, 3) { Scale = 1f };
        var loader = new Yolo26SnapdragonXLoader(modelPath);

        for (var i = 0; i < 10; i++) await loader.RunInference(input);
        var baseline = PrivateBytes();

        for (var i = 0; i < 1000; i++) await loader.RunInference(input);

        Assert.True(PrivateBytes() <= baseline + (64L * 1024L * 1024L));
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "EdgeAIKiosk1")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find repository root.");
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
