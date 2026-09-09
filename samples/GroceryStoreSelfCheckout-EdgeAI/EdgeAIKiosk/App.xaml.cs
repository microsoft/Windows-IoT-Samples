using EdgeAIKiosk.Views;
using Microsoft.UI.Xaml;

namespace EdgeAIKiosk
{
    public partial class App : Application
    {
        private Window? _window;

        public App() => InitializeComponent();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new HomeWindow();
            _window.Activate();
        }
    }
}
