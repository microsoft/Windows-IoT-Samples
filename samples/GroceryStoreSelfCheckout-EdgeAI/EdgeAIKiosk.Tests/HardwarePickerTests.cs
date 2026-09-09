using EdgeAIKiosk.Views;
using Microsoft.ML.OnnxRuntime;

namespace EdgeAIKiosk.Tests;

public class HardwarePickerTests
{
    [Fact]
    public void BuildHardwareOptions_ShowsAutoThenDetectedHardwareInDisplayOrder()
    {
        var options = HomeWindow.BuildHardwareOptions(
            [OrtHardwareDeviceType.NPU, OrtHardwareDeviceType.CPU]);

        Assert.Collection(
            options,
            option =>
            {
                Assert.Equal("Auto", option.Name);
                Assert.Null(option.Value);
            },
            option =>
            {
                Assert.Equal("CPU", option.Name);
                Assert.Equal(OrtHardwareDeviceType.CPU, option.Value);
            },
            option =>
            {
                Assert.Equal("NPU", option.Name);
                Assert.Equal(OrtHardwareDeviceType.NPU, option.Value);
            });
    }
}
