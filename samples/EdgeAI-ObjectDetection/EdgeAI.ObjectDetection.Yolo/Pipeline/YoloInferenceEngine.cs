// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EdgeAI.ObjectDetection.Yolo.Pipeline;

internal sealed class YoloInferenceEngine : IDisposable
{
    internal const int InputSize = 640;
    internal const int DetectionLimit = 300;

    private static readonly string[] CocoClasses =
    [
        "person", "bicycle", "car", "motorcycle", "airplane",
        "bus", "train", "truck", "boat", "traffic light",
        "fire hydrant", "stop sign", "parking meter", "bench", "bird",
        "cat", "dog", "horse", "sheep", "cow",
        "elephant", "bear", "zebra", "giraffe", "backpack",
        "umbrella", "handbag", "tie", "suitcase", "frisbee",
        "skis", "snowboard", "sports ball", "kite", "baseball bat",
        "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle",
        "wine glass", "cup", "fork", "knife", "spoon",
        "bowl", "banana", "apple", "sandwich", "orange",
        "broccoli", "carrot", "hot dog", "pizza", "donut",
        "cake", "chair", "couch", "potted plant", "bed",
        "dining table", "toilet", "tv", "laptop", "mouse",
        "remote", "keyboard", "cell phone", "microwave", "oven",
        "toaster", "sink", "refrigerator", "book", "clock",
        "vase", "scissors", "teddy bear", "hair drier", "toothbrush"
    ];

    private readonly InferenceSession _session;
    private readonly FrozenSet<int>? _enabledClassIds;
    private float[]? _inputBuffer;
    private NamedOnnxValue[] _inputs = Array.Empty<NamedOnnxValue>();
    public static IReadOnlyList<string> SupportedClasses
    {
        get;
    } = Array.AsReadOnly(CocoClasses);

    public string ExecutionProvider
    {
        get;
    }
    public string HardwareDevice
    {
        get;
    }

    private YoloInferenceEngine(InferenceSession session, string executionProvider, string hardwareDevice,
        FrozenSet<int>? enabledClassIds)
    {
        _session = session;
        _enabledClassIds = enabledClassIds;
        ExecutionProvider = executionProvider;
        HardwareDevice = hardwareDevice;
    }

    public static YoloInferenceEngine Create(string modelPath, OrtEpDevice selectedDevice,
        IReadOnlyCollection<string>? classMask = null)
    {
        FrozenSet<int>? enabledClassIds = ResolveClassMask(classMask);
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model file not found: {modelPath}", modelPath);
        }

        InferenceSession session;
        using (SessionOptions options = new())
        {
            options.AppendExecutionProvider(OrtEnv.Instance(), [selectedDevice], new Dictionary<string, string>());
            session = new InferenceSession(modelPath, options);
        }
        try
        {
            if (session.InputMetadata.Count != 1 || session.OutputMetadata.Count != 1)
            {
                throw new InvalidOperationException("Expected exactly one YOLO input and one output.");
            }

            NodeMetadata input = session.InputMetadata.Values.Single();
            NodeMetadata output = session.OutputMetadata.Values.Single();
            if (!input.IsTensor || input.ElementType != typeof(float) ||
                !output.IsTensor || output.ElementType != typeof(float))
            {
                throw new InvalidOperationException("Expected float32 input and output tensors.");
            }

            ValidateShapes(input.Dimensions, output.Dimensions);
            return new YoloInferenceEngine(session, selectedDevice.EpName,
                selectedDevice.HardwareDevice.Type.ToString(), enabledClassIds);
        }
        catch
        {
            session.Dispose();
            throw;
        }
    }

    internal static FrozenSet<int>? ResolveClassMask(IReadOnlyCollection<string>? classMask)
    {
        if (classMask is null)
        {
            return null;
        }

        HashSet<string> selectedClasses = new(classMask, StringComparer.OrdinalIgnoreCase);
        List<int> enabledClassIds = new();
        for (int classId = 0; classId < SupportedClasses.Count; classId++)
        {
            if (selectedClasses.Contains(SupportedClasses[classId]))
            {
                enabledClassIds.Add(classId);
            }
        }
        return enabledClassIds.ToFrozenSet();
    }

    internal static void ValidateShapes(IEnumerable<int> input, IEnumerable<int> output)
    {
        if (!input.SequenceEqual(new[] { 1, 3, InputSize, InputSize }) ||
            !output.SequenceEqual(new[] { 1, DetectionLimit, 6 }))
        {
            throw new InvalidOperationException(
                "Supported model contract: float32 input [1,3,640,640] and output [1,300,6], processed COCO detections.");
        }
    }

    public IReadOnlyList<ImageDetection> Run(PreprocessedFrame input)
    {
        if (!ReferenceEquals(_inputBuffer, input.Tensor))
        {
            DenseTensor<float> tensor = new(input.Tensor, [1, 3, InputSize, InputSize]);
            _inputs = [NamedOnnxValue.CreateFromTensor(_session.InputMetadata.Keys.Single(), tensor)];
            _inputBuffer = input.Tensor;
        }
        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs = _session.Run(_inputs);
        DisposableNamedOnnxValue output = outputs.FirstOrDefault()
            ?? throw new InvalidOperationException("The selected model returned no output tensors.");
        return ParseDetections(output.AsTensor<float>(), input.Letterbox, _enabledClassIds);
    }

    internal static IReadOnlyList<ImageDetection> ParseDetections(Tensor<float> output, LetterboxGeometry letterbox,
        IReadOnlySet<int>? enabledClassIds = null)
    {
        if (output.Rank != 3 || output.Dimensions[0] != 1 ||
            output.Dimensions[1] != DetectionLimit || output.Dimensions[2] != 6)
        {
            throw new InvalidOperationException(
                "Expected processed YOLO output [1,300,6] (x1, y1, x2, y2, confidence, COCO class id).");
        }

        List<ImageDetection> detections = new();
        for (int index = 0; index < output.Dimensions[1]; index++)
        {
            ImageDetection? detection = DecodeDetection(
                output[0, index, 0], output[0, index, 1], output[0, index, 2], output[0, index, 3],
                output[0, index, 4], output[0, index, 5], letterbox, enabledClassIds);
            if (detection is not null)
            {
                detections.Add(detection);
            }
        }

        return detections.AsReadOnly();
    }

    internal static ImageDetection? DecodeDetection(float x1, float y1, float x2, float y2,
        float confidence, float classId, LetterboxGeometry letterbox, IReadOnlySet<int>? enabledClassIds = null)
    {
        if (!float.IsFinite(confidence) || confidence < 0.5f || confidence > 1 ||
            !float.IsFinite(classId) || classId < 0 || classId >= SupportedClasses.Count || classId != (int)classId ||
            !float.IsFinite(x1) || !float.IsFinite(y1) || !float.IsFinite(x2) || !float.IsFinite(y2))
        {
            return null;
        }

        if (enabledClassIds is not null && !enabledClassIds.Contains((int)classId))
        {
            return null;
        }

        x1 = Math.Max(x1, letterbox.Left);
        y1 = Math.Max(y1, letterbox.Top);
        x2 = Math.Min(x2, letterbox.Left + letterbox.Width);
        y2 = Math.Min(y2, letterbox.Top + letterbox.Height);
        if (x2 <= x1 || y2 <= y1)
        {
            return null;
        }
        ImageDetectionBox box = new(x1, y1, x2 - x1, y2 - y1);
        return new ImageDetection((int)classId, SupportedClasses[(int)classId], confidence, box);
    }

    public void Dispose() => _session.Dispose();
}
