using EdgeAIKiosk1.Views;
using Microsoft.UI.Xaml;

namespace EdgeAIKiosk1
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
