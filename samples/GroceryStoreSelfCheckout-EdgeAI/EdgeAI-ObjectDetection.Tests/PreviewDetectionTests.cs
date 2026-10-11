// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using EdgeAI_ObjectDetection.Controls;

namespace EdgeAI_ObjectDetection.Tests;

public sealed class PreviewDetectionTests
{
    private static readonly Detection ImageDetection =
        new(0, "person", .9f, new DetectionBox(160, 200, 320, 240));

    [Fact]
    public void LivePreviewRemovesModelPadding()
    {
        var geometry = PreviewGeometry.Create(640, 480, 640, 480, 1);
        var detection = Assert.Single(geometry.MapDetections([ImageDetection], LetterboxGeometry.Create(640, 480)));

        Assert.Equal(new PreviewDetectionBox(160, 120, 320, 240), detection.BoundingBox);
        Assert.Equal(ImageDetection.ClassId, detection.ClassId);
        Assert.Equal(ImageDetection.Label, detection.Label);
        Assert.Equal(ImageDetection.Confidence, detection.Confidence);
    }

    [Fact]
    public void LivePreviewIncludesDisplayOffsetsAndScale()
    {
        var geometry = PreviewGeometry.Create(640, 480, 1280, 1280, 2);
        var detection = Assert.Single(geometry.MapDetections([ImageDetection], LetterboxGeometry.Create(640, 480)));

        Assert.Equal(new PreviewDetectionBox(320, 400, 640, 480), detection.BoundingBox);
    }

    [Fact]
    public void CroppedPreviewMapsToVisibleRegion()
    {
        var geometry = PreviewGeometry.Create(1920, 1080, 400, 800, 800.0 / 1080);
        Detection detection = new(1, "bicycle", .8f, new DetectionBox(240, 160, 160, 320));
        var padding = LetterboxGeometry.Create(geometry.CropWidth, geometry.CropHeight);
        var box = Assert.Single(geometry.MapDetections([detection], padding)).BoundingBox;

        Assert.Equal(100, box.X, 5);
        Assert.Equal(200, box.Y, 5);
        Assert.Equal(200, box.Width, 5);
        Assert.Equal(400, box.Height, 5);
    }

    [Fact]
    public void SnapshotDisplayPreservesImagePaddingAndImageCoordinates()
    {
        var geometry = PreviewGeometry.FitSquare(640, 800, 600);
        var detection = Assert.Single(geometry.MapDetections([ImageDetection], null));

        Assert.Equal(new PreviewDetectionBox(250, 187.5, 300, 225), detection.BoundingBox);
        Assert.Equal(new DetectionBox(160, 200, 320, 240), ImageDetection.BoundingBox);
    }

    [Fact]
    public void FractionalImagePixelsMapToDipsWithoutRounding()
    {
        Detection detection = new(0, "person", .9f, new DetectionBox(160.25f, 200.5f, 320.5f, 239.25f));
        var live = PreviewGeometry.Create(640, 480, 320, 240, .5);
        var still = PreviewGeometry.FitSquare(640, 320, 240);

        Assert.Equal(new PreviewDetectionBox(80.125, 60.25, 160.25, 119.625),
            Assert.Single(live.MapDetections([detection], LetterboxGeometry.Create(640, 480))).BoundingBox);
        Assert.Equal(new PreviewDetectionBox(100.09375, 75.1875, 120.1875, 89.71875),
            Assert.Single(still.MapDetections([detection], null)).BoundingBox);
    }

    [Fact]
    public void ResizingProducesNewCoordinatesWithoutMutatingPreviousResults()
    {
        var first = PreviewGeometry.FitSquare(640, 640, 640).MapDetections([ImageDetection], null);
        var resized = PreviewGeometry.FitSquare(640, 1280, 1280).MapDetections([ImageDetection], null);

        Assert.Equal(new PreviewDetectionBox(160, 200, 320, 240), first[0].BoundingBox);
        Assert.Equal(new PreviewDetectionBox(320, 400, 640, 480), resized[0].BoundingBox);
        Assert.Throws<NotSupportedException>(() => ((IList<PreviewDetection>)first).Clear());
        Assert.Empty(PreviewGeometry.FitSquare(640, 640, 640).MapDetections([], null));
    }
}
