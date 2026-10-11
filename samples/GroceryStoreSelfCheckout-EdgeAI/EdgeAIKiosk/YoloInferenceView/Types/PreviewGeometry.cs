// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace EdgeAI_ObjectDetection.Controls;

/// <summary>
/// Describes the visible rectangular crop of the camera frame and its preview placement.
/// Crop coordinates and dimensions are in source-frame pixels.
/// <see cref="Scale"/> converts source pixels to layout units; <see cref="ImageX"/> and
/// <see cref="ImageY"/> give the full frame's top-left relative to the viewport's top-left.
/// Overlay coordinates and dimensions describe the crop in layout units.
/// </summary>
internal readonly record struct PreviewGeometry(
    int CropX, int CropY, int CropWidth, int CropHeight,
    double Scale, double ImageX, double ImageY)
{
    public double OverlayX => ImageX + CropX * Scale;
    public double OverlayY => ImageY + CropY * Scale;
    public double OverlayWidth => CropWidth * Scale;
    public double OverlayHeight => CropHeight * Scale;

    internal IReadOnlyList<PreviewDetection> MapDetections(
        IReadOnlyList<Detection> detections, LetterboxGeometry? letterbox)
    {
        var mapped = new List<PreviewDetection>(detections.Count);
        double scaleX = OverlayWidth / (letterbox?.Width ?? CropWidth);
        double scaleY = OverlayHeight / (letterbox?.Height ?? CropHeight);
        int left = letterbox?.Left ?? 0;
        int top = letterbox?.Top ?? 0;
        foreach (Detection detection in detections)
        {
            DetectionBox box = detection.BoundingBox;
            mapped.Add(new PreviewDetection(detection.ClassId, detection.Label, detection.Confidence,
                new PreviewDetectionBox(OverlayX + (box.X - left) * scaleX, OverlayY + (box.Y - top) * scaleY,
                    box.Width * scaleX, box.Height * scaleY)));
        }
        return mapped.AsReadOnly();
    }

    public static PreviewGeometry FitSquare(int imageSize, double viewportWidth, double viewportHeight)
    {
        if (imageSize <= 0 || !double.IsFinite(viewportWidth) || !double.IsFinite(viewportHeight) ||
            viewportWidth <= 0 || viewportHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Image layout must be finite and positive.");
        }

        double size = Math.Min(viewportWidth, viewportHeight);
        return new(0, 0, imageSize, imageSize, size / imageSize,
            (viewportWidth - size) / 2, (viewportHeight - size) / 2);
    }

    // Round inward so even fractional layout sizes cannot include off-preview pixels.
    public static PreviewGeometry Create(int sourceWidth, int sourceHeight,
        double viewportWidth, double viewportHeight, double scale)
    {
        if (sourceWidth <= 0 || sourceHeight <= 0 ||
            !double.IsFinite(viewportWidth) || !double.IsFinite(viewportHeight) ||
            !double.IsFinite(scale) || viewportWidth <= 0 || viewportHeight <= 0 || scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Preview geometry must be finite and positive.");
        }

        double imageX = (viewportWidth - sourceWidth * scale) / 2;
        double imageY = (viewportHeight - sourceHeight * scale) / 2;
        int left = (int)Math.Ceiling(Math.Max(0, -imageX / scale));
        int top = (int)Math.Ceiling(Math.Max(0, -imageY / scale));
        int right = (int)Math.Floor(Math.Min(sourceWidth, (viewportWidth - imageX) / scale));
        int bottom = (int)Math.Floor(Math.Min(sourceHeight, (viewportHeight - imageY) / scale));
        if (right <= left || bottom <= top)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Preview must contain at least one source pixel.");
        }

        return new(left, top, right - left, bottom - top, scale, imageX, imageY);
    }
}
