using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EdgeAIKiosk1.Interfaces;
using EdgeAIKiosk1.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace EdgeAIKiosk1.Pipeline;

public sealed class Yolo26SnapdragonXLoader : IModelLoader
{
    // ONNX session state stores the loaded model and selected execution provider.
    private readonly InferenceSession _inferenceSession;
    public string ModelPath { get; private set; } = string.Empty;
    public string ExecutionProvider { get; private set; } = string.Empty;
    public string HardwareDevice { get; private set; } = string.Empty;

    /// <summary>Loads the ONNX model using the first working NPU, GPU, or CPU provider.</summary>
    /// <param name="modelPath">Path to the YOLO26 ONNX model file.</param>
    public Yolo26SnapdragonXLoader(string modelPath, OrtHardwareDeviceType? preferredHardware = null)
    {
        ModelPath = modelPath;
        (_inferenceSession, ExecutionProvider, HardwareDevice) = this.BuildSession(modelPath, preferredHardware);
    }

    #region Model startup

    /// <summary>Creates the ONNX Runtime session and configures provider-specific options.</summary>
    private (InferenceSession Session, string Provider, string Hardware) BuildSession(
        string modelPath,
        OrtHardwareDeviceType? preferredHardware)
    {
        if (!File.Exists(modelPath))
            throw new FileNotFoundException($"Model file not found: {modelPath}", modelPath);

        var devices = OrtEnv.Instance().GetEpDevices()
            .OrderByDescending(device => device.HardwareDevice.Type)
            .ThenBy(device => device.EpName)
            .ToList();
        if (preferredHardware is not null)
        {
            var selectedDevice = devices.FirstOrDefault(device => device.HardwareDevice.Type == preferredHardware.Value)
                ?? throw new InvalidOperationException($"{preferredHardware} execution provider is not available.");
            var selectedSession = CreateSession(modelPath, selectedDevice);
            return (selectedSession, selectedDevice.EpName, selectedDevice.HardwareDevice.Type.ToString());
        }

        var cpuDevice = devices.FirstOrDefault(device => device.HardwareDevice.Type == OrtHardwareDeviceType.CPU)
            ?? throw new InvalidOperationException("Windows ML did not provide a CPU execution provider.");
        var cpuSession = CreateSession(modelPath, cpuDevice);
        foreach (var device in devices)
        {
            if (device.HardwareDevice.Type == OrtHardwareDeviceType.CPU) continue;
            try
            {
                var session = CreateSession(modelPath, device);
                cpuSession.Dispose();
                return (session, device.EpName, device.HardwareDevice.Type.ToString());
            }
            catch (Exception exception) when (IsProviderFailure(exception))
            {
                Trace.TraceWarning($"Execution provider {device.EpName} failed: {exception.Message}");
            }
        }

        return (cpuSession, cpuDevice.EpName, OrtHardwareDeviceType.CPU.ToString());
    }

    private static InferenceSession CreateSession(string modelPath, OrtEpDevice device)
    {
        using var options = new SessionOptions();
        options.AppendExecutionProvider(OrtEnv.Instance(), [device], new Dictionary<string, string>());
        return new InferenceSession(modelPath, options);
    }

    private static bool IsProviderFailure(Exception exception) =>
        exception is OnnxRuntimeException or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException;

    #endregion

    #region Main entrypoint

    /// <summary>Runs YOLO inference and converts raw model output into detected items.</summary>
    /// <param name="input">The preprocessed tensor plus letterbox metadata from <see cref="Yolo26Preprocessor"/>.</param>
    public async Task<ModelOutput> RunInference(ModelInput input)
    {
        var inferenceStartTime = DateTime.UtcNow;
        using var inferenceOutputs = await this.RunSession(input);
        return this.ParseTimedOutputs(inferenceOutputs, input, inferenceStartTime);
    }

    #endregion

    #region Steps

    /// <summary>Executes the loaded ONNX session on the supplied model input.</summary>
    /// <param name="input">The model tensor, width, height, channel count, and letterbox values for one camera frame.</param>
    private async Task<IDisposableReadOnlyCollection<DisposableNamedOnnxValue>> RunSession(ModelInput input)
    {
        return await Task.Run(() =>
        {
            // Batch size is fixed at 1 because the kiosk verifies the current basket,
            // not an offline image batch; this keeps latency and memory predictable.
            var dimensions = new[] { 1, input.Channels, input.Height, input.Width };
            var tensor = new DenseTensor<float>(input.Tensor, dimensions);
            var sessionInput = NamedOnnxValue.CreateFromTensor("images", tensor);
            return this._inferenceSession.Run([sessionInput]);
        });
    }

    /// <summary>Parses ONNX outputs and attaches elapsed inference time.</summary>
    /// <param name="outputs">The raw named tensors returned by ONNX Runtime.</param>
    /// <param name="input">The original model input used to map detections back out of letterbox space.</param>
    private ModelOutput ParseTimedOutputs(
        IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs,
        ModelInput input,
        DateTime inferenceStartTime)
    {
        // Read the first ONNX output tensor and parse known YOLO26 shape.
        var tensor = outputs.First().AsTensor<float>();
        var detections = this.ParseYolo26(tensor, input);

        // Return detections with measured inference time.
        return new(detections, (DateTime.UtcNow - inferenceStartTime).TotalMilliseconds);
    }

    #endregion

    #region Helpers

    /// <summary>Converts YOLO26 fixed-slot tensor rows into detected items.</summary>
    /// <param name="tensor">The raw YOLO output tensor with boxes, confidence, and class ids.</param>
    /// <param name="input">The model input carrying letterbox padding and scale for box conversion.</param>
    private List<DetectedItem> ParseYolo26(Tensor<float> tensor, ModelInput input)
    {
        var detections = new List<DetectedItem>();
        // YOLO26 emits a fixed number of candidate slots; most are empty for any one frame.
        for (int detectionIndex = 0; detectionIndex < 300; detectionIndex++)
        {
            // Empty slots contain near-zero scores, so this light cutoff removes tensor noise
            // before the higher verification confidence gate runs.
            float confidence = tensor[0, detectionIndex, 4];
            if (confidence < 0.01f) continue;

            if (!TryGetCocoLabel(tensor[0, detectionIndex, 5], out var label)) continue;

            // Convert output coordinates and class id into a DetectedItem.
            var boundingBox = UnletterboxBox(tensor[0, detectionIndex, 0], tensor[0, detectionIndex, 1], tensor[0, detectionIndex, 2], tensor[0, detectionIndex, 3], input);
            var detection = new DetectedItem();
            detection.Label = label;
            detection.Confidence = confidence;
            detection.Box = boundingBox;
            detections.Add(detection);
        }
        return detections;
    }

    internal static bool TryGetCocoLabel(float classIdValue, out string label)
    {
        var classId = (int)classIdValue;
        if (classId < 0 || classId >= CocoLabels.Labels.Length)
        {
            label = string.Empty;
            return false;
        }

        label = CocoLabels.Labels[classId];
        return true;
    }

    /// <summary>Converts one YOLO box from letterboxed model coordinates to camera-frame coordinates.</summary>
    /// <param name="input">The preprocessed input that stores padding and scale from letterbox resize.</param>
    internal static BoundingBox UnletterboxBox(float x1r, float y1r, float x2r, float y2r, ModelInput input)
    {
        // Remove letterbox padding and scale back to original frame space.
        var x1 = Unscale(x1r, input.PadLeft, input.Scale);
        var y1 = Unscale(y1r, input.PadTop, input.Scale);
        var x2 = Unscale(x2r, input.PadLeft, input.Scale);
        var y2 = Unscale(y2r, input.PadTop, input.Scale);
        return new BoundingBox(x1, y1, x2 - x1, y2 - y1);
    }

    private static float Unscale(float coord, float pad, float scale) => (coord - pad) / scale;

    #endregion

    public void Dispose() => _inferenceSession.Dispose();
}
