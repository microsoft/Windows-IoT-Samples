// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using EdgeAI_ObjectDetection.Pipeline;

namespace EdgeAI_ObjectDetection.Tests;

public sealed class YoloPipelineTests
{
    [Fact]
    public async Task DisposeBeforeStartingCanBeRepeated()
    {
        var pipeline = new YoloPipeline();

        await pipeline.DisposeAsync();
        await pipeline.DisposeAsync();

        Assert.Null(pipeline.FrameSource);
    }
}
