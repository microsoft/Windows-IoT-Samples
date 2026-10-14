// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace EdgeAI.ObjectDetection.Yolo;

/// <summary>Coordinates in pixels relative to the 640x640 snapshot image's top-left, including letterbox padding. Fractional pixels are preserved.</summary>
public readonly record struct DetectionBox(float X, float Y, float Width, float Height);

public sealed record Detection(int ClassId, string Label, float Confidence, DetectionBox BoundingBox);

/// <summary>Coordinates in XAML DIPs relative to the preview area's top-left, excluding status text.</summary>
public readonly record struct PreviewDetectionBox(double X, double Y, double Width, double Height);

public sealed record PreviewDetection(int ClassId, string Label, float Confidence, PreviewDetectionBox BoundingBox);

/// <summary>Display coordinates for the layout at the time of this event; not snapshot-image coordinates.</summary>
public sealed class DetectionsUpdatedEventArgs(IReadOnlyList<PreviewDetection> detections) : EventArgs
{
    public IReadOnlyList<PreviewDetection> Detections
    {
        get;
    } = detections;
}

public sealed class InferenceFaultedEventArgs(Exception exception) : EventArgs
{
    public Exception Exception
    {
        get;
    } = exception;
}
