using System.Text.Json;
using DualOperator.Models;

namespace DualOperator.Helpers
{
    internal static class LoadOperator
    {
        public static List<RunningApp> OperatorApps = new List<RunningApp>();

        public static List<OperatorApp>? AppList;

        public static void LoadApps()
        {
            // First, get the config info
            AppList = JsonSerializer.Deserialize<List<OperatorApp>>(File.ReadAllText("operators.json"));

            // Sanity check - did we get anything
            if (AppList == null || AppList.Count == 0)
            {
                return;
            }

            // Process the first item
            RunningApp appItem = new RunningApp
            {
                Keyboard = AppList[0].Keyboard,
                WindowHandle = IntPtr.Zero,
                WindowTitle = AppList[0].ApplicationTitle
            };
            OperatorApps.Add(appItem);

            // And now the second item
            if (AppList.Count <= 1)
            {
                return;
            }

            appItem = new RunningApp
            {
                Keyboard = AppList[1].Keyboard,
                WindowHandle = IntPtr.Zero,
                WindowTitle = AppList[1].ApplicationTitle
            };
            OperatorApps.Add(appItem);
        }
    }
}
