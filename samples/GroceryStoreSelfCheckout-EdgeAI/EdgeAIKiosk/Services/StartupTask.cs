using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EdgeAIKiosk.Services;

public static class StartupTask
{
    public static async Task Run(Func<Task> task, string failureMessage, Action<string> showError, Action<Exception>? log = null)
    {
        try { await task(); }
        catch (Exception exception)
        {
            (log ?? (error => Trace.TraceError(error.ToString())))(exception);
            showError($"{failureMessage}: {exception.Message}");
        }
    }
}
