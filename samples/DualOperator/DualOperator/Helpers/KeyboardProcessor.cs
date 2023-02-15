using System.Runtime.InteropServices;
using System.Text;
using DualOperator.Models;

namespace DualOperator.Helpers
{
    internal static class KeyboardProcessor
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(CallBackPtr lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)] 
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private delegate bool CallBackPtr(int hwnd, int lParam);

        private static CallBackPtr callBackPtr = Callback;
        private static List<WinStruct> windowList = new List<WinStruct>();

        public static void SendMessageToWindow(string Key, string WindowTitle)
        {
            // Get our window handle
            WinStruct doWindow = GetWindows().FirstOrDefault(x => x.WinTitle.ToUpper().Contains("DUAL OPERATOR"))!;

            // Get the main window handle for the target app and send the message
            WinStruct appWindow = GetWindows().FirstOrDefault(x => x.WinTitle.ToUpper().Contains(WindowTitle.ToUpper()))!;
            SetForegroundWindow((IntPtr)appWindow.MainWindowHandle);
            SendKeys.SendWait(Key);

            // Return focus to us
            if (doWindow.MainWindowHandle != 0)
            {
                SetForegroundWindow((IntPtr) doWindow.MainWindowHandle);
            }
        }

        private static bool Callback(int hWnd, int lparam)
        {
            StringBuilder sb = new StringBuilder(256);
            int res = GetWindowText((IntPtr)hWnd, sb, 256);
            windowList.Add(new WinStruct { MainWindowHandle = hWnd, WinTitle = sb.ToString() });
            return true;
        }

        private static List<WinStruct> GetWindows()
        {
            windowList = new List<WinStruct>();
            EnumWindows(callBackPtr, IntPtr.Zero);
            return windowList;
        }

    }
}
