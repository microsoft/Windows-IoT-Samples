using EdgeAI.ObjectDetection.Yolo;

namespace EdgeAI.SelfCheckoutKiosk.Tests;

public class KioskSettingsTests
{
    [Fact]
    public void NewSettings_HaveNoPreferredModel()
    {
        KioskSettings settings = new();

        Assert.Null(settings.ModelFileName);
    }

    [Fact]
    public void NewSettings_AcceptAllSupportedClassesIgnoringCase()
    {
        KioskSettings settings = new();
        IReadOnlyList<string> classes = YoloInferenceView.GetSupportedClasses();

        Assert.Equal(classes.Count, settings.AcceptedLabels.Count);
        foreach (string label in classes)
        {
            Assert.Contains(label.ToUpperInvariant(), settings.AcceptedLabels);
        }
    }

    [Fact]
    public void ChangingSelection_DoesNotChangeOtherSettingsOrSupportedClasses()
    {
        KioskSettings settings = new();
        KioskSettings otherSettings = new();

        settings.AcceptedLabels.Clear();

        Assert.NotEmpty(otherSettings.AcceptedLabels);
        Assert.True(otherSettings.AcceptedLabels.SetEquals(YoloInferenceView.GetSupportedClasses()));
    }
}
