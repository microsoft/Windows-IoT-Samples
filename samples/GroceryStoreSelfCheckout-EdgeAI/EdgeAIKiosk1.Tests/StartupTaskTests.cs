using EdgeAIKiosk1.Services;

namespace EdgeAIKiosk1.Tests;

public class StartupTaskTests
{
    [Fact]
    public async Task Run_ShowsAndLogsFailureWithoutRethrowing()
    {
        var exception = new InvalidOperationException("camera missing");
        Exception? logged = null;
        string? shown = null;

        await StartupTask.Run(
            () => Task.FromException(exception),
            "Camera initialization failed",
            message => shown = message,
            error => logged = error);

        Assert.Same(exception, logged);
        Assert.Contains("Camera initialization failed", shown);
        Assert.Contains("camera missing", shown);
    }

    [Fact]
    public async Task Run_DoesNothingOnSuccess()
    {
        var ran = false;
        var showedError = false;

        await StartupTask.Run(
            () =>
            {
                ran = true;
                return Task.CompletedTask;
            },
            "Should not appear",
            _ => showedError = true,
            _ => throw new InvalidOperationException("should not log"));

        Assert.True(ran);
        Assert.False(showedError);
    }
}
