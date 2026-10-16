using EdgeAI.ObjectDetection.Yolo.Pipeline;

namespace EdgeAI.ObjectDetection.Yolo.Tests;

public sealed class DetectionDecoderTests
{
    private static readonly LetterboxGeometry SquareInput = LetterboxGeometry.Create(640, 640);

    [Fact]
    public void BoxesUseSnapshotImagePixels()
    {
        var detection = YoloInferenceEngine.DecodeDetection(160, 80, 480, 400, .8f, 1, SquareInput);
        Assert.NotNull(detection);
        Assert.Equal(1, detection.ClassId);
        Assert.Equal("bicycle", detection.Label);
        Assert.Equal(new ImageDetectionBox(160, 80, 320, 320), detection.BoundingBox);
    }

    [Fact]
    public void BoxesAreClippedToCrop()
    {
        var detection = YoloInferenceEngine.DecodeDetection(-10, -20, 650, 700, .8f, 0, SquareInput);
        Assert.NotNull(detection);
        Assert.Equal(new ImageDetectionBox(0, 0, 640, 640), detection.BoundingBox);
    }

    [Fact]
    public void FractionalPixelCoordinatesArePreserved()
    {
        var detection = YoloInferenceEngine.DecodeDetection(160.25f, 80.5f, 480.75f, 400.125f, .8f, 1, SquareInput);
        Assert.NotNull(detection);
        Assert.Equal(new ImageDetectionBox(160.25f, 80.5f, 320.5f, 319.625f), detection.BoundingBox);
    }

    [Theory]
    [InlineData(.49f, 0)]
    [InlineData(float.NaN, 0)]
    [InlineData(1.1f, 0)]
    [InlineData(.8f, -1)]
    [InlineData(.8f, 80)]
    [InlineData(.8f, .5f)]
    [InlineData(.8f, float.PositiveInfinity)]
    public void InvalidOrLowConfidenceRowsAreIgnored(float confidence, float classId) =>
        Assert.Null(YoloInferenceEngine.DecodeDetection(0, 0, 100, 100, confidence, classId, SquareInput));

    [Theory]
    [InlineData(float.NaN, 0, 100, 100)]
    [InlineData(100, 0, 0, 100)]
    [InlineData(0, 0, 100, 0)]
    [InlineData(650, 0, 700, 100)]
    public void InvalidOrEmptyBoxesAreIgnored(float x1, float y1, float x2, float y2) =>
        Assert.Null(YoloInferenceEngine.DecodeDetection(x1, y1, x2, y2, .8f, 0, SquareInput));
}
