using EdgeAIKiosk.Pipeline;

namespace EdgeAIKiosk.Tests;

public class HardwareFailureTests
{
    [Fact]
    public void SelectCameraId_ThrowsMeaningfulError_WhenNoCameraExists()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ImageCapture.SelectCameraId([], null));

        Assert.Contains("No camera detected", exception.Message);
    }

    [Fact]
    public void SelectCameraId_FallsBackToFirstCamera_WhenSavedCameraIsMissing()
    {
        var selected = ImageCapture.SelectCameraId(["camera-1", "camera-2"], "missing");

        Assert.Equal("camera-1", selected);
    }

    [Fact]
    public void Constructor_ThrowsMeaningfulError_WhenModelFileIsMissing()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".onnx");

        var exception = Assert.Throws<FileNotFoundException>(() =>
            new Yolo26SnapdragonXLoader(missingPath));

        Assert.Contains("Model file not found", exception.Message);
    }

    [Fact]
    public void TryGetCocoLabel_ReturnsFalse_ForInvalidClassId()
    {
        var found = Yolo26SnapdragonXLoader.TryGetCocoLabel(999, out var label);

        Assert.False(found);
        Assert.Equal(string.Empty, label);
    }
}
