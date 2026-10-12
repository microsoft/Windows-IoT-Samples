using EdgeAI_ObjectDetection.Controls;
using EdgeAI_ObjectDetection.Pipeline;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Frozen;

namespace EdgeAI_ObjectDetection.Tests;

public class DetectionFilterTests
{
    private static IReadOnlyList<Detection> Decode(IReadOnlySet<int>? enabledClassIds)
    {
        DenseTensor<float> output = new(new[] { 1, 300, 6 });
        int[] classIds = [47, 0, 46];
        for (int row = 0; row < classIds.Length; row++)
        {
            output[0, row, 0] = 64;
            output[0, row, 1] = 128;
            output[0, row, 2] = 256;
            output[0, row, 3] = 384;
            output[0, row, 4] = .9f;
            output[0, row, 5] = classIds[row];
        }
        return YoloInferenceEngine.ParseDetections(output, LetterboxGeometry.Create(640, 640), enabledClassIds);
    }

    [Fact]
    public void NullMask_IncludesAllClasses()
    {
        FrozenSet<int>? mask = YoloInferenceEngine.ResolveClassMask(null);
        Assert.Null(mask);
        Assert.Equal(new[] { "apple", "person", "banana" }, Decode(mask).Select(detection => detection.Label));
    }

    [Fact]
    public void EmptySelection_ReturnsNoDetections()
    {
        FrozenSet<int>? mask = YoloInferenceEngine.ResolveClassMask(Array.Empty<string>());
        Assert.NotNull(mask);
        Assert.Empty(Decode(mask));
    }

    [Fact]
    public void ClassNames_AreResolvedOnceCaseInsensitivelyWithoutRenumberingClasses()
    {
        List<string> classes = new() { "APPLE", "banana", "apple" };
        FrozenSet<int>? mask = YoloInferenceEngine.ResolveClassMask(classes);
        classes.Clear();
        classes.Add("person");
        IReadOnlyList<Detection> result = Decode(mask);
        Assert.Equal(new[] { "apple", "banana" }, result.Select(detection => detection.Label));
        Assert.Equal(new[] { 47, 46 }, result.Select(detection => detection.ClassId));
        Assert.All(result, detection =>
            Assert.Equal(new DetectionBox(64, 128, 192, 256), detection.BoundingBox));
        Assert.Throws<NotSupportedException>(() => ((IList<Detection>)result).Clear());
    }

    [Fact]
    public void Discovery_ReturnsAllClassesRegardlessOfMask()
    {
        IReadOnlyList<string> classes = YoloInferenceView.GetSupportedClasses();
        Assert.Equal(80, classes.Count);
        Assert.Equal("person", classes[0]);
        Assert.Equal("toothbrush", classes[79]);
        string[] originalClasses = classes.ToArray();
        FrozenSet<int>? firstMask = YoloInferenceEngine.ResolveClassMask(new[] { "apple" });
        FrozenSet<int>? secondMask = YoloInferenceEngine.ResolveClassMask(new[] { "person" });
        Assert.Equal("apple", Assert.Single(Decode(firstMask)).Label);
        Assert.Equal("person", Assert.Single(Decode(secondMask)).Label);
        Assert.Empty(Decode(YoloInferenceEngine.ResolveClassMask(Array.Empty<string>())));
        Assert.Equal(originalClasses, YoloInferenceView.GetSupportedClasses());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)classes).Clear());
    }

    [Fact]
    public void AllSupportedClasses_AreAccepted()
    {
        FrozenSet<int>? mask = YoloInferenceEngine.ResolveClassMask(YoloInferenceView.GetSupportedClasses());
        Assert.NotNull(mask);
        Assert.Equal(Enumerable.Range(0, 80), mask.Order());
        Assert.Equal(3, Decode(mask).Count);
    }

    [Theory]
    [InlineData("not a supported class")]
    [InlineData("")]
    [InlineData(null)]
    public void UnsupportedClass_ReturnsNoDetections(string className)
    {
        FrozenSet<int>? mask = YoloInferenceEngine.ResolveClassMask(new[] { className });
        Assert.NotNull(mask);
        Assert.Empty(mask);
        Assert.Empty(Decode(mask));
    }

    [Theory]
    [InlineData("not a supported class")]
    [InlineData("")]
    [InlineData(null)]
    public void UnsupportedClass_DoesNotAffectSupportedSelections(string className)
    {
        FrozenSet<int>? mask = YoloInferenceEngine.ResolveClassMask(new[] { className, "APPLE", "apple" });
        Assert.NotNull(mask);
        Assert.Equal(47, Assert.Single(mask));
        Assert.Equal("apple", Assert.Single(Decode(mask)).Label);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(80)]
    [InlineData(.5f)]
    [InlineData(float.NaN)]
    public void MaskedDecoder_StillRejectsInvalidClassIds(float classId)
    {
        FrozenSet<int>? mask = YoloInferenceEngine.ResolveClassMask(new[] { "apple" });
        Assert.Null(YoloInferenceEngine.DecodeDetection(0, 0, 640, 640, .9f, classId,
            LetterboxGeometry.Create(640, 640), mask));
    }
}
