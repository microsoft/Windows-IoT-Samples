// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using EdgeAI.ObjectDetection.Yolo.Pipeline;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace EdgeAI.ObjectDetection.Yolo;

/// <summary>A one-shot result whose image is owned and disposed by the caller.</summary>
public sealed class InferenceSnapshot : IDisposable
{
    private SoftwareBitmap? _image;

    /// <summary>Detections with bounding boxes in pixels of <see cref="Image"/>, independent of preview layout.</summary>
    public IReadOnlyList<Detection> Detections { get; }

    /// <summary>The 640x640 model-input image, in BGRA8 premultiplied format for XAML display.</summary>
    public SoftwareBitmap Image => _image ?? throw new ObjectDisposedException(nameof(InferenceSnapshot));

    internal InferenceSnapshot(IReadOnlyList<Detection> detections, byte[] imagePixels)
    {
        Detections = detections;
        _image = SoftwareBitmap.CreateCopyFromBuffer(imagePixels.AsBuffer(),
            BitmapPixelFormat.Bgra8, YoloInferenceEngine.InputSize, YoloInferenceEngine.InputSize,
            BitmapAlphaMode.Premultiplied);
    }

    public void Dispose()
    {
        _image?.Dispose();
        _image = null;
    }
}
