using EdgeAI.ObjectDetection.Yolo;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;

namespace EdgeAIKiosk.Services;

internal sealed record KioskInferenceOptions(
    string ModelPath,
    MediaFrameSourceGroup Camera,
    OrtHardwareDeviceType? PreferredHardware,
    IReadOnlySet<string> SelectedClasses);

internal static class KioskInference
{
    internal static Task StartAsync(YoloInferenceView view, KioskInferenceOptions options,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return ExecutionProviderPolicy.StartAsync(OrtEnv.Instance().GetEpDevices(),
            device => device.HardwareDevice.Type, device => device.EpName, options.PreferredHardware,
            device => view.StartAsync(new InferenceSettings
            {
                ModelPath = options.ModelPath,
                FrameSourceGroup = options.Camera,
                ExecutionProvider = device,
                ClassMask = options.SelectedClasses,
                InferenceType = InferenceType.Stream,
                MaxEndToEndFps = 30
            }, token), token);
    }
}
