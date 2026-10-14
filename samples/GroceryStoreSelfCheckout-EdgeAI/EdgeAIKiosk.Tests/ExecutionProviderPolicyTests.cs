using EdgeAIKiosk.Services;
using Microsoft.ML.OnnxRuntime;
using System.Runtime.InteropServices;

namespace EdgeAIKiosk.Tests;

public class ExecutionProviderPolicyTests
{
    private sealed record Device(string Name, OrtHardwareDeviceType Type);
    private static readonly Device[] Devices =
    [
        new("cpu", OrtHardwareDeviceType.CPU),
        new("gpu", OrtHardwareDeviceType.GPU),
        new("npu", OrtHardwareDeviceType.NPU)
    ];

    private static Task Start(OrtHardwareDeviceType? preferred, Func<Device, Task> start,
        CancellationToken token = default, IEnumerable<Device>? devices = null) =>
        ExecutionProviderPolicy.StartAsync(devices ?? Devices, device => device.Type,
            device => device.Name, preferred, start, token);

    [Theory]
    [InlineData("npu")]
    [InlineData("gpu")]
    [InlineData("cpu")]
    public async Task Auto_UsesFirstWorkingProviderInPriorityOrder(string working)
    {
        List<string> attempts = new();
        await Start(null, device =>
        {
            attempts.Add(device.Name);
            return device.Name == working ? Task.CompletedTask
                : Task.FromException(new DllNotFoundException("provider unavailable"));
        });
        Assert.Equal(new[] { "npu", "gpu", "cpu" }.Take(attempts.Count), attempts);
        Assert.Equal(working, attempts.Last());
    }

    [Theory]
    [InlineData(OrtHardwareDeviceType.NPU)]
    [InlineData(OrtHardwareDeviceType.GPU)]
    [InlineData(OrtHardwareDeviceType.CPU)]
    public async Task ExplicitHardware_DoesNotFallBack(OrtHardwareDeviceType hardware)
    {
        List<Device> attempts = new();
        DllNotFoundException error = new("provider unavailable");
        Assert.Same(error, await Assert.ThrowsAsync<DllNotFoundException>(() => Start(hardware, device =>
        {
            attempts.Add(device);
            return Task.FromException(error);
        })));
        Assert.Equal(hardware, Assert.Single(attempts).Type);
    }

    [Fact]
    public async Task Auto_DoesNotHideCameraOrModelErrors()
    {
        foreach (Exception error in new Exception[]
        {
            new COMException("camera denied"),
            new FileNotFoundException("model missing"),
            new InvalidOperationException("invalid model shape"),
            new OperationCanceledException()
        })
        {
            int attempts = 0;
            Exception? actual = await Record.ExceptionAsync(() => Start(null, _ =>
            {
                attempts++;
                return Task.FromException(error);
            }));
            Assert.Same(error, actual);
            Assert.Equal(1, attempts);
        }
    }

    [Fact]
    public async Task Auto_ReportsAllProviderFailures()
    {
        AggregateException error = await Assert.ThrowsAsync<AggregateException>(() =>
            Start(null, _ => Task.FromException(new BadImageFormatException())));
        Assert.Equal(3, error.InnerExceptions.Count);
    }

    [Fact]
    public async Task CancelledStart_DoesNotTryAnotherProvider()
    {
        using CancellationTokenSource cancellation = new();
        int attempts = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Start(null, _ =>
        {
            attempts++;
            cancellation.Cancel();
            return Task.FromException(new EntryPointNotFoundException());
        }, cancellation.Token));
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task MissingHardware_ReportsUnavailableWithoutStarting()
    {
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Start(OrtHardwareDeviceType.NPU, _ => throw new Exception("must not start"),
                devices: [Devices[0]]));
        Assert.Contains("NPU", error.Message);
    }
}
