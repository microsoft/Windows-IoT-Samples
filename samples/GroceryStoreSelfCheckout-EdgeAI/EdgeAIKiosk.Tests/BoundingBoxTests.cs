using EdgeAIKiosk.Models;

namespace EdgeAIKiosk.Tests;

public class BoundingBoxTests
{
    [Fact]
    public void Area_Equals_WidthTimesHeight()
    {
        var box = new BoundingBox(10f, 20f, 100f, 50f);
        Assert.Equal(5000f, box.Area);
    }

    [Fact]
    public void Area_IsZero_WhenWidthIsZero()
    {
        var box = new BoundingBox(0f, 0f, 0f, 50f);
        Assert.Equal(0f, box.Area);
    }

    [Fact]
    public void Area_IsZero_WhenHeightIsZero()
    {
        var box = new BoundingBox(0f, 0f, 100f, 0f);
        Assert.Equal(0f, box.Area);
    }

    [Fact]
    public void Area_IsZero_WhenBothDimensionsAreZero()
    {
        var box = new BoundingBox(5f, 5f, 0f, 0f);
        Assert.Equal(0f, box.Area);
    }
}
