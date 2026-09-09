using EdgeAIKiosk.Models;
using EdgeAIKiosk.Pipeline;

namespace EdgeAIKiosk.Tests;

public class MajorityFramesTests
{
    private static DetectedItem Item(string label, float confidence = 0.9f) =>
        new() { Label = label, Confidence = confidence };

    [Fact]
    public void Validate_ReturnsEmpty_WhenRequiredCountNotMet()
    {
        var mf = new MajorityFrames { RequiredCount = 3 };
        mf.Track(new[] { Item("apple") });
        var result = mf.Track(new[] { Item("apple") });
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ReturnsItem_WhenRequiredCountMet()
    {
        var mf = new MajorityFrames { RequiredCount = 2 };
        mf.Track(new[] { Item("banana") });
        var result = mf.Track(new[] { Item("banana") });
        Assert.Single(result);
        Assert.Equal("banana", result[0].Label);
    }

    [Fact]
    public void Validate_PreservesBoundingBox_WhenRequiredCountMet()
    {
        var mf = new MajorityFrames { RequiredCount = 1 };
        var result = mf.Track(new[] { new DetectedItem { Label = "apple", Confidence = 0.9f, Box = new(1, 2, 3, 4) } });

        Assert.Equal(1, result[0].Box.X);
        Assert.Equal(2, result[0].Box.Y);
        Assert.Equal(3, result[0].Box.Width);
        Assert.Equal(4, result[0].Box.Height);
    }

    [Fact]
    public void Validate_FiltersItemsBelowMinConfidence()
    {
        var mf = new MajorityFrames { RequiredCount = 1, MinConfidence = 0.5f };
        var result = mf.Track(new[] { Item("orange", confidence: 0.3f) });
        Assert.Empty(result);
    }

    [Fact]
    public void Reset_ClearsAccumulatedState()
    {
        var mf = new MajorityFrames { RequiredCount = 1 };
        mf.Track(new[] { Item("apple") });
        var result = mf.Track(Array.Empty<DetectedItem>(), reset: true);
        Assert.Empty(result);
        Assert.Equal(0, mf.TotalObservations);
    }
}