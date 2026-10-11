using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EdgeAIKiosk.Services;

public static class StartupTask
{
    public static async Task Run(Func<Task> task, string failureMessage, Action<string> showError, Action<Exception>? log = null)
    {
        try
        {
            await task();
        }
        catch (Exception exception)
        {
            if (log is null)
            {
                Trace.TraceError(exception.ToString());
            }
            else
            {
                log(exception);
            }
            showError($"{failureMessage}: {exception.Message}");
        }
    }
}
