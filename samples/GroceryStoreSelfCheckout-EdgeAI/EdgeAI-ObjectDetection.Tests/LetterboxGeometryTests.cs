// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using EdgeAI_ObjectDetection.Controls;
using EdgeAI_ObjectDetection.Pipeline;

namespace EdgeAI_ObjectDetection.Tests;

public sealed class LetterboxGeometryTests
{
    [Theory]
    [InlineData(1920, 1080, 640, 360, 0, 140)]
    [InlineData(640, 480, 640, 480, 0, 80)]
    [InlineData(320, 240, 640, 480, 0, 80)]
    [InlineData(240, 320, 480, 640, 80, 0)]
    [InlineData(640, 479, 640, 479, 0, 80)]
    [InlineData(479, 640, 479, 640, 80, 0)]
    [InlineData(1080, 1080, 640, 640, 0, 0)]
    [InlineData(1, 1000, 1, 640, 319, 0)]
    public void ContentFitsWithIntegerPadding(int width, int height, int resizedWidth, int resizedHeight,
        int left, int top)
    {
        Assert.Equal(new LetterboxGeometry(resizedWidth, resizedHeight, left, top),
            LetterboxGeometry.Create(width, height));
    }

    [Theory]
    [InlineData(640, 480)]
    [InlineData(480, 640)]
    [InlineData(640, 479)]
    public void BoxesCrossingPaddingAreClippedAndMapToFullCrop(int width, int height)
    {
        var layout = LetterboxGeometry.Create(width, height);
        var detection = YoloInferenceEngine.DecodeDetection(0, 0, 640, 640, .9f, 0, layout);
        Assert.NotNull(detection);
        Assert.Equal(new DetectionBox(layout.Left, layout.Top, layout.Width, layout.Height), detection.BoundingBox);
        var geometry = PreviewGeometry.Create(width, height, width, height, 1);
        Assert.Equal(new PreviewDetectionBox(0, 0, width, height),
            Assert.Single(geometry.MapDetections([detection], layout)).BoundingBox);
    }

    [Theory]
    [InlineData(0, 0, 640, 80)]
    [InlineData(0, 560, 640, 640)]
    public void PaddingOnlyBoxesAreRejected(float x1, float y1, float x2, float y2)
    {
        var layout = LetterboxGeometry.Create(640, 480);
        Assert.Null(YoloInferenceEngine.DecodeDetection(x1, y1, x2, y2, .9f, 0, layout));
    }

    [Fact]
    public void LandscapeBoxMapsFromSnapshotToVisiblePreview()
    {
        var geometry = PreviewGeometry.Create(1920, 1080, 800, 400, 800.0 / 1920);
        var layout = LetterboxGeometry.Create(geometry.CropWidth, geometry.CropHeight);
        Assert.Equal(new LetterboxGeometry(640, 320, 0, 160), layout);
        var detection = YoloInferenceEngine.DecodeDetection(160, 240, 480, 400, .9f, 0, layout);
        Assert.NotNull(detection);
        // Snapshot coordinates retain the padding; streaming coordinates remove it.
        Assert.Equal(new DetectionBox(160, 240, 320, 160), detection.BoundingBox);
        var box = Assert.Single(geometry.MapDetections([detection], layout)).BoundingBox;
        Assert.Equal(200, box.X, 6);
        Assert.Equal(100, box.Y, 6);
        Assert.Equal(400, box.Width, 6);
        Assert.Equal(200, box.Height, 6);
    }

    [Fact]
    public void PortraitBoxMapsToVisiblePreview()
    {
        var geometry = PreviewGeometry.Create(1920, 1080, 400, 800, 800.0 / 1080);
        var layout = LetterboxGeometry.Create(geometry.CropWidth, geometry.CropHeight);
        Assert.Equal(new LetterboxGeometry(320, 640, 160, 0), layout);
        var detection = YoloInferenceEngine.DecodeDetection(240, 160, 400, 480, .9f, 0, layout);
        Assert.NotNull(detection);
        Assert.Equal(new PreviewDetectionBox(100, 200, 200, 400),
            Assert.Single(geometry.MapDetections([detection], layout)).BoundingBox);
    }
}
