// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using EdgeAI.ObjectDetection.Yolo.Pipeline;

namespace EdgeAI.ObjectDetection.Yolo.Tests;

public sealed class YoloPipelineTests
{
    [Fact]
    public void CameraCompatibilityRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => YoloInferenceView.IsSupportedCamera(null!));
    }

    [Fact]
    public async Task DisposeBeforeStartingCanBeRepeated()
    {
        var pipeline = new YoloPipeline();

        await pipeline.DisposeAsync();
        await pipeline.DisposeAsync();

        Assert.Null(pipeline.FrameSource);
    }
}
