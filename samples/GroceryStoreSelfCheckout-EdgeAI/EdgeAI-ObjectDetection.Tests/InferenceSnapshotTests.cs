using EdgeAI_ObjectDetection.Controls;
using EdgeAI_ObjectDetection.Pipeline;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace EdgeAI_ObjectDetection.Tests;

public sealed class InferenceSnapshotTests
{
    [Theory]
    [InlineData(80, 0, 640, 640)]
    [InlineData(240, 160, 320, 320)]
    [InlineData(0, 80, 800, 480)]
    [InlineData(240, 0, 320, 640)]
    public void SnapshotPixelsMatchEveryModelInputChannel(int cropX, int cropY, int cropWidth, int cropHeight)
    {
        using var frame = CreateFrame();
        var geometry = new PreviewGeometry(cropX, cropY, cropWidth, cropHeight, 1, 0, 0);
        var (input, _, pixels) = new YoloPreprocessor().Preprocess(frame, geometry, true);
        Assert.NotNull(pixels);

        using var snapshot = new InferenceSnapshot(Array.Empty<Detection>(), pixels);
        Assert.Equal(640, snapshot.Image.PixelWidth);
        Assert.Equal(640, snapshot.Image.PixelHeight);
        Assert.Equal(BitmapPixelFormat.Bgra8, snapshot.Image.BitmapPixelFormat);
        Assert.Equal(BitmapAlphaMode.Premultiplied, snapshot.Image.BitmapAlphaMode);
        byte[] actual = new byte[640 * 640 * 4];
        snapshot.Image.CopyToBuffer(actual.AsBuffer());

        for (int i = 0; i < 640 * 640; i++)
        {
            Assert.Equal(input[i], actual[i * 4 + 2] / 255f);
            Assert.Equal(input[640 * 640 + i], actual[i * 4 + 1] / 255f);
            Assert.Equal(input[2 * 640 * 640 + i], actual[i * 4] / 255f);
            Assert.Equal((byte)255, actual[i * 4 + 3]);
        }

        if (cropWidth == 640 && cropHeight == 640)
        {
            Assert.Equal((byte)cropX, actual[2]);
            Assert.Equal((byte)cropY, actual[1]);
            Assert.Equal((byte)((cropX + 639) % 256), actual[^2]);
            Assert.Equal((byte)((cropY + 639) % 256), actual[^3]);
        }
    }

    [Fact]
    public void StreamingDoesNotAllocateSnapshotPixels()
    {
        using var frame = CreateFrame();
        var geometry = new PreviewGeometry(80, 0, 640, 640, 1, 0, 0);
        var preprocessor = new YoloPreprocessor();
        var streaming = preprocessor.Preprocess(frame, geometry, false);
        var savedTensor = streaming.Tensor.ToArray();
        var oneShot = preprocessor.Preprocess(frame, geometry, true);

        Assert.Null(streaming.ImagePixels);
        Assert.NotNull(oneShot.ImagePixels);
        Assert.Equal(savedTensor, oneShot.Tensor);
        Assert.Equal(streaming.Letterbox, oneShot.Letterbox);
    }

    [Fact]
    public void SnapshotOwnsImageAndKeepsDetectionsAfterDisposal()
    {
        byte[] pixels;
        using (var frame = CreateFrame())
        {
            pixels = new YoloPreprocessor().Preprocess(
                frame, new PreviewGeometry(80, 0, 640, 640, 1, 0, 0), true).ImagePixels!;
        }

        var detection = YoloInferenceEngine.DecodeDetection(160, 80, 480, 400, .8f, 0,
            LetterboxGeometry.Create(640, 640));
        Assert.NotNull(detection);
        IReadOnlyList<Detection> detections = Array.AsReadOnly(new[] { detection });
        using var snapshot = new InferenceSnapshot(detections, pixels);
        Array.Clear(pixels);
        snapshot.Image.CopyToBuffer(pixels.AsBuffer());
        Assert.Equal((byte)80, pixels[2]);
        Assert.Equal((byte)255, pixels[3]);
        Assert.Same(detections, snapshot.Detections);
        Assert.Equal(new DetectionBox(160, 80, 320, 320), snapshot.Detections[0].BoundingBox);

        snapshot.Dispose();
        snapshot.Dispose();
        Assert.Throws<ObjectDisposedException>(() => snapshot.Image);
        Assert.Same(detections, snapshot.Detections);
    }

    private static SoftwareBitmap CreateFrame()
    {
        byte[] pixels = new byte[800 * 640 * 4];
        for (int y = 0; y < 640; y++)
        {
            for (int x = 0; x < 800; x++)
            {
                int offset = (y * 800 + x) * 4;
                pixels[offset] = (byte)((x + y) % 256);
                pixels[offset + 1] = (byte)(y % 256);
                pixels[offset + 2] = (byte)(x % 256);
                pixels[offset + 3] = 255;
            }
        }
        return SoftwareBitmap.CreateCopyFromBuffer(pixels.AsBuffer(), BitmapPixelFormat.Bgra8,
            800, 640, BitmapAlphaMode.Ignore);
    }
}
