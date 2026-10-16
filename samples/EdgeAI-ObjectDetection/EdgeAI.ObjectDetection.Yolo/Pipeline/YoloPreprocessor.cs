// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace EdgeAI.ObjectDetection.Yolo.Pipeline;

// One workspace per pipeline. Its tensor is borrowed until the next call; inference consumes it synchronously.
internal sealed class YoloPreprocessor
{
    private const int InputSize = YoloInferenceEngine.InputSize;
    private const int PlaneSize = InputSize * InputSize;
    private readonly float[] _tensor = new float[3 * PlaneSize];
    private byte[]? _snapshotPixels;
    private byte[] _source = Array.Empty<byte>();
    private IBuffer? _sourceBuffer;
    private float[] _horizontal = Array.Empty<float>();
    private Kernel[] _horizontalKernels = Array.Empty<Kernel>();
    private Kernel[] _verticalKernels = Array.Empty<Kernel>();
    private int _cropWidth;
    private int _cropHeight;

    private readonly record struct Kernel(int Start, float[] Weights);

    internal PreprocessedFrame Preprocess(SoftwareBitmap frame, PreviewGeometry crop, bool captureImage)
    {
        int width = frame.PixelWidth;
        int height = frame.PixelHeight;
        if (crop.CropWidth <= 0 || crop.CropHeight <= 0 ||
            crop.CropWidth > width || crop.CropHeight > height ||
            crop.CropX < 0 || crop.CropY < 0 ||
            crop.CropX > width - crop.CropWidth || crop.CropY > height - crop.CropHeight)
        {
            throw new ArgumentOutOfRangeException(nameof(crop), "The crop must fit inside the camera frame.");
        }

        int sourceLength = checked(width * height * 4);
        if (_source.Length != sourceLength)
        {
            _source = new byte[sourceLength];
            _sourceBuffer = _source.AsBuffer();
        }

        // The camera reader requests BGRA8. Only non-native formats/alpha modes need a conversion.
        if (frame.BitmapPixelFormat == BitmapPixelFormat.Bgra8 &&
            frame.BitmapAlphaMode == BitmapAlphaMode.Ignore)
        {
            frame.CopyToBuffer(_sourceBuffer!);
        }
        else
        {
            using SoftwareBitmap converted = SoftwareBitmap.Convert(frame, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
            converted.CopyToBuffer(_sourceBuffer!);
        }
        byte[]? imagePixels = null;
        if (captureImage)
        {
            _snapshotPixels ??= new byte[4 * PlaneSize];
            imagePixels = _snapshotPixels;
        }
        LetterboxGeometry letterbox = LetterboxGeometry.Create(crop.CropWidth, crop.CropHeight);
        if (letterbox.Width != InputSize || letterbox.Height != InputSize)
        {
            Array.Fill(_tensor, 114 / 255f);
            if (imagePixels is not null)
            {
                for (int i = 0; i < PlaneSize; i++)
                {
                    int offset = i * 4;
                    imagePixels[offset] = 114;
                    imagePixels[offset + 1] = 114;
                    imagePixels[offset + 2] = 114;
                    imagePixels[offset + 3] = 255;
                }
            }
        }

        if (crop.CropWidth == letterbox.Width && crop.CropHeight == letterbox.Height)
        {
            for (int y = 0; y < crop.CropHeight; y++)
            {
                int source = ((crop.CropY + y) * width + crop.CropX) * 4;
                for (int x = 0; x < crop.CropWidth; x++, source += 4)
                {
                    StorePixel((y + letterbox.Top) * InputSize + x + letterbox.Left, _source[source], _source[source + 1],
                        _source[source + 2], imagePixels);
                }
            }
            return new(_tensor, letterbox, imagePixels);
        }

        if (_cropWidth != crop.CropWidth || _cropHeight != crop.CropHeight)
        {
            int scratchLength = checked(crop.CropHeight * letterbox.Width * 3);
            _horizontalKernels = BuildKernels(crop.CropWidth, letterbox.Width);
            _verticalKernels = BuildKernels(crop.CropHeight, letterbox.Height);
            _horizontal = new float[scratchLength];
            _cropWidth = crop.CropWidth;
            _cropHeight = crop.CropHeight;
        }

        ResizeHorizontal(width, crop, letterbox.Width);
        ResizeVertical(letterbox, imagePixels);
        return new(_tensor, letterbox, imagePixels);
    }

    private static Kernel[] BuildKernels(int size, int outputSize)
    {
        Kernel[] kernels = new Kernel[outputSize];
        double ratio = (double)size / outputSize;
        double filterScale = Math.Max(1, ratio);
        double radius = 2 * filterScale;
        for (int destination = 0; destination < outputSize; destination++)
        {
            double center = (destination + 0.5) * ratio - 0.5;
            int first = Math.Max(0, (int)Math.Ceiling(center - radius));
            int last = Math.Min(size - 1, (int)Math.Floor(center + radius));
            float[] weights = new float[last - first + 1];
            double sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                double weight = Cubic((first + i - center) / filterScale);
                weights[i] = (float)weight;
                sum += weight;
            }
            // Truncate at the crop boundary, then normalize to preserve constant colors.
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = (float)(weights[i] / sum);
            }
            kernels[destination] = new Kernel(first, weights);
        }
        return kernels;
    }

    // Catmull-Rom cubic convolution (a = -0.5). Widen support when shrinking to suppress aliasing.
    private static double Cubic(double distance)
    {
        double x = Math.Abs(distance);
        if (x <= 1) return 1 + x * x * (1.5 * x - 2.5);
        if (x < 2) return -0.5 * (x - 1) * (x - 2) * (x - 2);
        return 0;
    }

    private void ResizeHorizontal(int width, PreviewGeometry crop, int outputWidth)
    {
        for (int y = 0; y < crop.CropHeight; y++)
        {
            int row = ((crop.CropY + y) * width + crop.CropX) * 4;
            for (int x = 0; x < outputWidth; x++)
            {
                Kernel kernel = _horizontalKernels[x];
                int source = row + kernel.Start * 4;
                float b = 0, g = 0, r = 0;
                foreach (float weight in kernel.Weights)
                {
                    b += _source[source] * weight;
                    g += _source[source + 1] * weight;
                    r += _source[source + 2] * weight;
                    source += 4;
                }
                int target = (y * outputWidth + x) * 3;
                _horizontal[target] = b;
                _horizontal[target + 1] = g;
                _horizontal[target + 2] = r;
            }
        }
    }

    private void ResizeVertical(LetterboxGeometry letterbox, byte[]? imagePixels)
    {
        for (int y = 0; y < letterbox.Height; y++)
        {
            Kernel kernel = _verticalKernels[y];
            for (int x = 0; x < letterbox.Width; x++)
            {
                int source = (kernel.Start * letterbox.Width + x) * 3;
                float b = 0, g = 0, r = 0;
                foreach (float weight in kernel.Weights)
                {
                    b += _horizontal[source] * weight;
                    g += _horizontal[source + 1] * weight;
                    r += _horizontal[source + 2] * weight;
                    source += letterbox.Width * 3;
                }
                StorePixel((y + letterbox.Top) * InputSize + x + letterbox.Left,
                    Quantize(b), Quantize(g), Quantize(r), imagePixels);
            }
        }
    }

    private static byte Quantize(float channel) => (byte)Math.Clamp((int)(channel + 0.5f), 0, 255);

    private void StorePixel(int index, byte b, byte g, byte r, byte[]? imagePixels)
    {
        _tensor[index] = r / 255f;
        _tensor[PlaneSize + index] = g / 255f;
        _tensor[2 * PlaneSize + index] = b / 255f;
        if (imagePixels is not null)
        {
            int offset = index * 4;
            imagePixels[offset] = b;
            imagePixels[offset + 1] = g;
            imagePixels[offset + 2] = r;
            imagePixels[offset + 3] = 255;
        }
    }
}

// Buffers are borrowed until the next preprocessing call. InferenceSnapshot copies image pixels before then.
internal readonly record struct PreprocessedFrame(float[] Tensor, LetterboxGeometry Letterbox, byte[]? ImagePixels);
