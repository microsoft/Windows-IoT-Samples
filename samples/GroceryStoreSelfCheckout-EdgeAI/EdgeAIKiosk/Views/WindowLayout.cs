using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace EdgeAIKiosk;

public static class WindowLayout
{
    public static void Maximize(Window window)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow.Presenter is OverlappedPresenter presenter) presenter.Maximize();
    }
}
