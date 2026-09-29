using EdgeAIKiosk.Models;
using EdgeAIKiosk.Pipeline;

namespace EdgeAIKiosk.Tests;

public class MajorityFramesTests
{
    private static DetectedItem Item(string label, float confidence = 0.9f) =>
        new() { Label = label, Confidence = confidence };

    [Fact]
    public void Track_AcceptsItemOnlyAtThreshold_AndPreservesLatestDetection()
    {
        var mf = new MajorityFrames { RequiredCount = 3 };
        Assert.Empty(mf.Track(new[] { Item("apple") }));
        Assert.Empty(mf.Track(new[] { Item("apple") }));

        var latest = new DetectedItem { Label = "apple", Confidence = 0.8f, Box = new(1, 2, 3, 4) };
        var item = Assert.Single(mf.Track(new[] { latest }));
        Assert.Equal(latest.Label, item.Label);
        Assert.Equal(latest.Confidence, item.Confidence);
        Assert.Equal(latest.Box, item.Box);
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
        var mf = new MajorityFrames { RequiredCount = 2 };
        mf.Track(new[] { Item("apple") });
        Assert.Single(mf.Track(new[] { Item("apple") }));

        Assert.Empty(mf.Track(Array.Empty<DetectedItem>(), reset: true));
        Assert.Empty(mf.Track(new[] { Item("apple") }));
        var item = Assert.Single(mf.Track(new[] { Item("apple") }));
        Assert.Equal("apple", item.Label);
    }
}