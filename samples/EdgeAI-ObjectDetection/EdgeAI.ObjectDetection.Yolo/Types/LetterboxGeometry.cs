// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using EdgeAI.ObjectDetection.Yolo.Pipeline;
using System;

namespace EdgeAI.ObjectDetection.Yolo;

// Shared integer dimensions keep resampling, padding, and box mapping in agreement.
internal readonly record struct LetterboxGeometry(int Width, int Height, int Left, int Top)
{
    private const int InputSize = YoloInferenceEngine.InputSize;

    public static LetterboxGeometry Create(int cropWidth, int cropHeight)
    {
        if (cropWidth <= 0 || cropHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cropWidth), "Crop dimensions must be positive.");
        }
        double scale = (double)InputSize / Math.Max(cropWidth, cropHeight);
        int width = Math.Clamp((int)Math.Round(cropWidth * scale), 1, InputSize);
        int height = Math.Clamp((int)Math.Round(cropHeight * scale), 1, InputSize);
        return new(width, height, (InputSize - width) / 2, (InputSize - height) / 2);
    }
}
