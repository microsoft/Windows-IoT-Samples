using EdgeAIKiosk1.Views;

namespace EdgeAIKiosk1.Tests;

public class LiveInferenceDisplayTests
{
    [Fact]
    public void IsScanned_MatchesLabelsCaseInsensitively()
    {
        var scannedLabels = new HashSet<string>(["Bottle"], StringComparer.OrdinalIgnoreCase);

        Assert.True(LiveInferenceView.IsScanned("bottle", scannedLabels));
        Assert.False(LiveInferenceView.IsScanned("cup", scannedLabels));
    }

    [Fact]
    public void DetachAndDispose_DetachesBeforeDisposing()
    {
        var calls = new List<string>();

        LiveInferenceView.DetachAndDispose(
            () => calls.Add("detach"),
            new CallbackDisposable(() => calls.Add("dispose")));

        Assert.Equal(["detach", "dispose"], calls);
    }

    private sealed class CallbackDisposable(Action callback) : IDisposable
    {
        public void Dispose() => callback();
    }
}
