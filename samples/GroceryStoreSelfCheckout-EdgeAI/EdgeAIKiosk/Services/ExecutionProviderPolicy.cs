using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeAIKiosk.Services;

internal static class ExecutionProviderPolicy
{
    internal static async Task StartAsync<T>(
        IEnumerable<T> devices,
        Func<T, OrtHardwareDeviceType> hardwareType,
        Func<T, string> providerName,
        OrtHardwareDeviceType? preferredHardware,
        Func<T, Task> start,
        CancellationToken token)
    {
        List<T> candidates = devices
            .Where(device => preferredHardware is null || hardwareType(device) == preferredHardware)
            .OrderBy(device => hardwareType(device) switch
            {
                OrtHardwareDeviceType.NPU => 0,
                OrtHardwareDeviceType.GPU => 1,
                OrtHardwareDeviceType.CPU => 2,
                _ => 3
            })
            .ThenBy(providerName, StringComparer.Ordinal)
            .ToList();
        token.ThrowIfCancellationRequested();
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException($"{preferredHardware?.ToString() ?? "Windows ML"} execution provider is not available.");
        }

        List<Exception> failures = new();
        foreach (T device in candidates)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                await start(device);
                token.ThrowIfCancellationRequested();
                return;
            }
            catch (Exception error) when (preferredHardware is null && IsProviderFailure(error))
            {
                Trace.TraceWarning($"Execution provider {providerName(device)} failed: {error.Message}");
                failures.Add(error);
            }
        }
        token.ThrowIfCancellationRequested();
        throw new AggregateException("No execution provider could load the model.", failures);
    }

    private static bool IsProviderFailure(Exception error) =>
        error is OnnxRuntimeException or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException;
}
