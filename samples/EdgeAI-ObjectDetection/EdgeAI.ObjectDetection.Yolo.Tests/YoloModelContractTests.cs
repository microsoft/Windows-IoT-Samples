using EdgeAI.ObjectDetection.Yolo.Pipeline;

namespace EdgeAI.ObjectDetection.Yolo.Tests;

public sealed class YoloModelContractTests
{
    [Fact]
    public void AcceptsSingleBatchProcessedDetections() =>
        YoloInferenceEngine.ValidateShapes([1, 3, 640, 640], [1, 300, 6]);

    [Theory]
    [InlineData(2, 640)]
    [InlineData(-1, 640)]
    [InlineData(1, 320)]
    public void RejectsUnsupportedBatchOrInputSize(int batch, int size) =>
        Assert.Throws<InvalidOperationException>(() =>
            YoloInferenceEngine.ValidateShapes([batch, 3, size, size], [1, 300, 6]));

    [Theory]
    [InlineData(1, 84, 8400)]
    [InlineData(1, 300, 85)]
    [InlineData(1, -1, 6)]
    [InlineData(2, 300, 6)]
    public void RejectsRawDynamicOrDifferentOutputs(int batch, int rows, int columns) =>
        Assert.Throws<InvalidOperationException>(() =>
            YoloInferenceEngine.ValidateShapes([1, 3, 640, 640], [batch, rows, columns]));
}
