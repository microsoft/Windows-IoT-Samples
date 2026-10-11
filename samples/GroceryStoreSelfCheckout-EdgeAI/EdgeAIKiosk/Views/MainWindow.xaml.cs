using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace EdgeAIKiosk.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.Maximize();
        }
        RootFrame.Navigate(typeof(HomePage));
    }
}
