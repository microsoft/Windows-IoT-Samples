using EdgeAIKiosk.Models;
using EdgeAIKiosk.Pipeline;

namespace EdgeAIKiosk.Tests;

public class LetterboxMathTests
{
    [Fact]
    public void UnletterboxBox_NoPaddingScaleOne_ReturnsOriginalCoords()
    {
        var input = new ModelInput(Array.Empty<float>(), 640, 640, 3)
            { PadLeft = 0f, PadTop = 0f, Scale = 1f };

        var result = Yolo26SnapdragonXLoader.UnletterboxBox(0f, 0f, 640f, 640f, input);

        Assert.Equal(0f,   result.X);
        Assert.Equal(0f,   result.Y);
        Assert.Equal(640f, result.Width);
        Assert.Equal(640f, result.Height);
    }

    [Fact]
    public void UnletterboxBox_WithPadLeftAndHalfScale_RemovesPaddingAndUpscales()
    {
        var input = new ModelInput(Array.Empty<float>(), 640, 640, 3)
            { PadLeft = 80f, PadTop = 0f, Scale = 0.5f };

        var result = Yolo26SnapdragonXLoader.UnletterboxBox(80f, 0f, 180f, 100f, input);

        Assert.Equal(0f,   result.X);
        Assert.Equal(0f,   result.Y);
        Assert.Equal(200f, result.Width);
        Assert.Equal(200f, result.Height);
    }
}
