// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using EdgeAI.ObjectDetection.Yolo.Pipeline;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace EdgeAI.ObjectDetection.Yolo.Tests;

public sealed class PreprocessorTests
{
    [Theory]
    [InlineData(640, 480, 640, 480, 0, 80)]
    [InlineData(320, 240, 640, 480, 0, 80)]
    [InlineData(960, 720, 640, 480, 0, 80)]
    [InlineData(240, 320, 480, 640, 80, 0)]
    [InlineData(640, 479, 640, 479, 0, 80)]
    [InlineData(1, 1000, 1, 640, 319, 0)]
    public void RectangularCropHasOpaqueGrayPadding(int width, int height, int resizedWidth,
        int resizedHeight, int left, int top)
    {
        using var frame = CreateFrame(width + 4, height + 2,
            (x, y) => x >= 2 && x < width + 2 && y >= 1 && y < height + 1
                ? ((byte)19, (byte)87, (byte)203) : ((byte)255, (byte)0, (byte)0));
        var (input, layout, pixels) = new YoloPreprocessor().Preprocess(
            frame, new PreviewGeometry(2, 1, width, height, 1, 0, 0), true);
        Assert.Equal(new LetterboxGeometry(resizedWidth, resizedHeight, left, top), layout);
        Assert.NotNull(pixels);
        var detection = YoloInferenceEngine.DecodeDetection(0, 0, 640, 640, .9f, 0, layout);
        Assert.NotNull(detection);
        Assert.Equal(new ImageDetectionBox(left, top, resizedWidth, resizedHeight),
            detection.BoundingBox);
        var geometry = PreviewGeometry.Create(width, height, width, height, 1);
        Assert.Equal(new PreviewDetectionBox(0, 0, width, height),
            Assert.Single(geometry.MapDetections([detection], layout)).BoundingBox);
        for (int y = 0; y < 640; y++)
        {
            for (int x = 0; x < 640; x++)
            {
                bool content = x >= left && x < left + resizedWidth && y >= top && y < top + resizedHeight;
                int i = y * 640 + x;
                byte b = content ? (byte)19 : (byte)114;
                byte g = content ? (byte)87 : (byte)114;
                byte r = content ? (byte)203 : (byte)114;
                Assert.Equal(r / 255f, input[i]);
                Assert.Equal(g / 255f, input[640 * 640 + i]);
                Assert.Equal(b / 255f, input[2 * 640 * 640 + i]);
                Assert.Equal(b, pixels[i * 4]);
                Assert.Equal(g, pixels[i * 4 + 1]);
                Assert.Equal(r, pixels[i * 4 + 2]);
                Assert.Equal((byte)255, pixels[i * 4 + 3]);
            }
        }
    }

    [Fact]
    public void RectangularResizePreservesIndependentHorizontalAndVerticalGradients()
    {
        using var frame = CreateFrame(170, 90,
            (x, y) => ((byte)x, (byte)y, (byte)64));
        var (input, _, pixels) = new YoloPreprocessor().Preprocess(
            frame, new PreviewGeometry(5, 3, 160, 80, 1, 0, 0), true);

        // Fourfold enlargement places the 640x320 content at y=160.
        // Away from the crop edges, cubic interpolation reproduces a linear ramp.
        for (int y = 8; y < 312; y++)
        {
            for (int x = 8; x < 632; x++)
            {
                int i = (y + 160) * 640 + x;
                byte expectedB = (byte)Math.Round(5 + (x + .5) / 4 - .5);
                byte expectedG = (byte)Math.Round(3 + (y + .5) / 4 - .5);
                Assert.Equal(expectedB, pixels![i * 4]);
                Assert.Equal(expectedG, pixels[i * 4 + 1]);
                Assert.Equal(64 / 255f, input[i]);
            }
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(317)]
    [InlineData(640)]
    [InlineData(1080)]
    public void ConstantCropPreservesColorsAndExcludesSurroundingPixels(int size)
    {
        using var frame = CreateFrame(size + 4, size + 2,
            (x, y) => x >= 2 && x < size + 2 && y >= 1 && y < size + 1
                ? ((byte)19, (byte)87, (byte)203) : ((byte)255, (byte)0, (byte)0));
        var (input, _, pixels) = new YoloPreprocessor().Preprocess(
            frame, new PreviewGeometry(2, 1, size, size, 1, 0, 0), true);

        for (int i = 0; i < 640 * 640; i++)
        {
            Assert.Equal(203 / 255f, input[i]);
            Assert.Equal(87 / 255f, input[640 * 640 + i]);
            Assert.Equal(19 / 255f, input[2 * 640 * 640 + i]);
            Assert.Equal((byte)19, pixels![4 * i]);
            Assert.Equal((byte)255, pixels[4 * i + 3]);
        }
    }

    [Fact]
    public void DownsamplingCheckerboardSuppressesAliasing()
    {
        using var frame = CreateFrame(1920, 1920,
            (x, y) => ((byte)((x + y) % 2 * 255), (byte)0, (byte)0));
        var pixels = new YoloPreprocessor().Preprocess(
            frame, new PreviewGeometry(0, 0, 1920, 1920, 1, 0, 0), true).ImagePixels;
        for (int y = 4; y < 636; y++)
        {
            for (int x = 4; x < 636; x++)
            {
                Assert.InRange(pixels![(y * 640 + x) * 4], (byte)127, (byte)128);
            }
        }
    }

    [Theory]
    [InlineData(317, new int[] { 11, 0, 174, 11, 0, 178, 71, 255, 173, 11, 0, 181, 11, 0, 187,
        228, 0, 70, 230, 51, 94, 131, 255, 255, 191, 0, 246 })]
    [InlineData(639, new int[] { 11, 0, 178, 12, 0, 195, 137, 255, 16, 13, 0, 209, 14, 0, 226,
        199, 255, 106, 201, 255, 154, 7, 255, 244, 133, 0, 82 })]
    [InlineData(640, new int[] { 11, 0, 178, 12, 0, 195, 138, 0, 33, 13, 0, 209, 14, 0, 226,
        200, 255, 130, 203, 255, 178, 9, 255, 19, 136, 255, 130 })]
    [InlineData(641, new int[] { 11, 0, 178, 12, 1, 195, 139, 0, 50, 13, 0, 209, 14, 1, 226,
        201, 255, 154, 205, 127, 210, 11, 255, 50, 139, 255, 178 })]
    [InlineData(1080, new int[] { 12, 0, 207, 14, 204, 207, 66, 0, 94, 15, 0, 129, 17, 204, 50,
        91, 255, 186, 96, 153, 96, 121, 0, 81, 175, 0, 155 })]
    public void PatternMatchesBicubicReferenceAtEdgesAndInterior(int size, int[] expectedRgb)
    {
        using var frame = CreateFrame(size + 10, size + 6,
            (x, y) => ((byte)(x * 17 + y * 31), (byte)((x / 7 + y / 9) % 2 * 255), (byte)(x + 2 * y)));
        var input = new YoloPreprocessor().Preprocess(
            frame, new PreviewGeometry(5, 3, size, size, 1, 0, 0), false).Tensor;
        int[] positions = [0, 1, 639, 640, 641, 319 * 640 + 319, 320 * 640 + 320, 639 * 640, 640 * 640 - 1];

        // Captured from the former ImageSharp 3.1.12 public API before removing the dependency.
        // Allow one byte for floating-point accumulation/rounding differences.
        for (int sample = 0; sample < positions.Length; sample++)
        {
            for (int channel = 0; channel < 3; channel++)
            {
                int actual = (int)Math.Round(input[channel * 640 * 640 + positions[sample]] * 255);
                Assert.InRange(Math.Abs(actual - expectedRgb[sample * 3 + channel]), 0, size == 640 ? 0 : 1);
            }
        }
    }

    [Theory]
    [InlineData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore)]
    [InlineData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied)]
    [InlineData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight)]
    public void NonNativeFormatMatchesExplicitWindowsConversion(BitmapPixelFormat format, BitmapAlphaMode alpha)
    {
        byte[] bytes = new byte[8 * 8 * 4];
        for (int i = 0; i < bytes.Length; i += 4)
        {
            bytes[i] = 19;
            bytes[i + 1] = 47;
            bytes[i + 2] = 83;
            bytes[i + 3] = 127;
        }
        using var frame = SoftwareBitmap.CreateCopyFromBuffer(bytes.AsBuffer(), format, 8, 8, alpha);
        using var converted = SoftwareBitmap.Convert(frame, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
        var crop = new PreviewGeometry(0, 0, 8, 8, 1, 0, 0);
        var actual = new YoloPreprocessor().Preprocess(frame, crop, true);
        var expected = new YoloPreprocessor().Preprocess(converted, crop, true);
        Assert.Equal(expected.Tensor, actual.Tensor);
        Assert.Equal(expected.ImagePixels, actual.ImagePixels);
        Assert.Equal(expected.Letterbox, actual.Letterbox);
    }

    [Fact]
    public void ReusedWorkspaceHandlesFrameDimensionChanges()
    {
        var preprocessor = new YoloPreprocessor();
        var crop = new PreviewGeometry(2, 1, 317, 317, 1, 0, 0);
        foreach (var (width, height) in new[] { (640, 480), (480, 640), (800, 600), (320, 320) })
        {
            using var frame = CreateFrame(width, height, (x, y) => ((byte)x, (byte)y, (byte)(x + y)));
            var actual = preprocessor.Preprocess(frame, crop, false);
            var expected = new YoloPreprocessor().Preprocess(frame, crop, false);
            Assert.Equal(expected.Tensor, actual.Tensor);
            Assert.Equal(expected.Letterbox, actual.Letterbox);
        }
    }

    [Fact]
    public void ReusedWorkspaceHandlesNewCropsWithoutChangingCopiedSnapshot()
    {
        using var frame = CreateFrame(800, 720, (x, y) => ((byte)x, (byte)y, (byte)(x + y)));
        var preprocessor = new YoloPreprocessor();
        var (first, firstLayout, firstPixels) = preprocessor.Preprocess(
            frame, new PreviewGeometry(80, 40, 640, 640, 1, 0, 0), true);
        Assert.NotNull(firstPixels);
        var saved = firstPixels.ToArray();
        using var snapshot = new InferenceSnapshot(Array.Empty<ImageDetection>(), firstPixels);
        var snapshotPixels = new byte[saved.Length];
        foreach (var crop in new[]
        {
            new PreviewGeometry(20, 30, 317, 317, 1, 0, 0),
            new PreviewGeometry(30, 40, 317, 317, 1, 0, 0),
            new PreviewGeometry(0, 0, 720, 720, 1, 0, 0),
            new PreviewGeometry(0, 0, 800, 480, 1, 0, 0),
            new PreviewGeometry(10, 10, 480, 700, 1, 0, 0),
            new PreviewGeometry(80, 40, 640, 640, 1, 0, 0)
        })
        {
            var (actual, layout, pixels) = preprocessor.Preprocess(frame, crop, true);
            var (expected, expectedLayout, expectedPixels) = new YoloPreprocessor().Preprocess(frame, crop, true);
            Assert.Same(first, actual);
            Assert.Equal(expected, actual);
            Assert.Equal(expectedPixels, pixels);
            Assert.Equal(expectedLayout, layout);
            Assert.Equal(new LetterboxGeometry(640, 640, 0, 0), firstLayout);
            Assert.Same(firstPixels, pixels);
            snapshot.Image.CopyToBuffer(snapshotPixels.AsBuffer());
            Assert.Equal(saved, snapshotPixels);
        }
    }

    [Theory]
    [InlineData(-1, 0, 2)]
    [InlineData(0, -1, 2)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(7, 0, 2)]
    [InlineData(0, 7, 2)]
    [InlineData(0, 0, int.MaxValue)]
    [InlineData(int.MaxValue, 0, 1)]
    public void InvalidCropIsRejectedBeforeIndexing(int x, int y, int size)
    {
        using var frame = CreateFrame(8, 8, (_, _) => ((byte)0, (byte)0, (byte)0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new YoloPreprocessor().Preprocess(
                frame, new PreviewGeometry(x, y, size, size, 1, 0, 0), false));
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(4, 0)]
    [InlineData(9, 4)]
    [InlineData(4, 9)]
    [InlineData(4, int.MaxValue)]
    public void InvalidRectangularDimensionsAreRejected(int width, int height)
    {
        using var frame = CreateFrame(8, 8, (_, _) => ((byte)0, (byte)0, (byte)0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new YoloPreprocessor().Preprocess(
                frame, new PreviewGeometry(0, 0, width, height, 1, 0, 0), false));
    }

    [Theory]
    [InlineData(1080, 1080, false)]
    [InlineData(1080, 720, false)]
    [InlineData(720, 1080, false)]
    [InlineData(1080, 1080, true)]
    [InlineData(1080, 720, true)]
    [InlineData(720, 1080, true)]
    public void WarmPreprocessingReusesLargeBuffers(int width, int height, bool captureImage)
    {
        using var frame = CreateFrame(1080, 1080, (_, _) => ((byte)10, (byte)20, (byte)30));
        var preprocessor = new YoloPreprocessor();
        var crop = new PreviewGeometry(0, 0, width, height, 1, 0, 0);
        var first = preprocessor.Preprocess(frame, crop, captureImage);
        preprocessor.Preprocess(frame, crop, captureImage);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 3; i++)
        {
            var next = preprocessor.Preprocess(frame, crop, captureImage);
            Assert.Same(first.Tensor, next.Tensor);
            if (captureImage)
            {
                Assert.NotNull(next.ImagePixels);
                Assert.Same(first.ImagePixels, next.ImagePixels);
            }
            else
            {
                Assert.Null(next.ImagePixels);
            }
        }
        // Allow small WinRT interop allocations, but no per-frame pixel, scratch, or tensor arrays.
        Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0L, 16_384L);
    }

    private static SoftwareBitmap CreateFrame(int width, int height, Func<int, int, (byte B, byte G, byte R)> pixel)
    {
        byte[] bytes = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = pixel(x, y);
                int offset = (y * width + x) * 4;
                bytes[offset] = color.B;
                bytes[offset + 1] = color.G;
                bytes[offset + 2] = color.R;
                bytes[offset + 3] = 255;
            }
        }
        return SoftwareBitmap.CreateCopyFromBuffer(bytes.AsBuffer(), BitmapPixelFormat.Bgra8,
            width, height, BitmapAlphaMode.Ignore);
    }
}
