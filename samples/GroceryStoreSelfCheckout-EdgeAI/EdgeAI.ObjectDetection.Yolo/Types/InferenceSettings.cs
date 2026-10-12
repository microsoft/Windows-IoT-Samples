// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using Windows.Media.Capture.Frames;

namespace EdgeAI.ObjectDetection.Yolo;

public enum InferenceType
{
    Stream,
    OneShot
}
public enum YoloInferenceState
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Disposed
}

public sealed record InferenceSettings
{
    public required MediaFrameSourceGroup FrameSourceGroup
    {
        get;
        init;
    }
    public required string ModelPath
    {
        get;
        init;
    }
    public required OrtEpDevice ExecutionProvider
    {
        get;
        init;
    }
    public double MaxEndToEndFps
    {
        get;
        init;
    } = 30;
    public InferenceType InferenceType
    {
        get;
        init;
    } = InferenceType.Stream;

    /// <summary>
    /// Included class names, matched case-insensitively against GetSupportedClasses().
    /// Null includes all classes; an empty collection includes none.
    /// Unsupported names are ignored; a mask with no supported names includes none.
    /// Do not modify the collection during StartAsync; subsequent edits do not affect the active run.
    /// </summary>
    public IReadOnlyCollection<string>? ClassMask { get; init; }

    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(FrameSourceGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(ModelPath);
        ArgumentNullException.ThrowIfNull(ExecutionProvider);
        if (!double.IsFinite(MaxEndToEndFps) || MaxEndToEndFps < 0.1 || MaxEndToEndFps > 240)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxEndToEndFps), "Use a rate between 0.1 and 240 FPS.");
        }
        if (!Enum.IsDefined(InferenceType))
        {
            throw new ArgumentOutOfRangeException(nameof(InferenceType));
        }
    }
}
