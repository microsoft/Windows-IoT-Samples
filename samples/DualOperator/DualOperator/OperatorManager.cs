using DualOperator.Helpers;
using DualOperator.Models;
using DualOperator.Structures;

namespace DualOperator
{
    public partial class OperatorManager : Form
    {
        private readonly RawInput inputManager;

        // Key modifier states
        private readonly List<OperatorKeyState> KeyStateList = new List<OperatorKeyState>();

        // Running apps
        private readonly List<RunningApp> WatchedApps = LoadOperator.OperatorApps;

        public OperatorManager()
        {
            InitializeComponent();

            // Initialize the Input Manager
            this.inputManager = new RawInput(this.Handle, false);
            this.inputManager.KeyPressed += InputManager_KeyPressed;

            // Set the key state watchers
            foreach (RunningApp app in WatchedApps)
            {
                this.KeyStateList.Add(new OperatorKeyState
                {
                    Device = app.Keyboard,
                    AltPressed = false,
                    ControlPressed = false,
                    ShiftPressed = false
                });
            }
        }

        private void InputManager_KeyPressed(object sender, RawInputEventArg e)
        {
            // Get out if this is not a keyboard we are tracking
            if (e.KeyPressEvent.TargetApp == null)
            {
                return;
            }

            // Get the keyboard we are working with
            string originatingKeyboard = e.KeyPressEvent.TargetApp.Keyboard;

            // Determine if we have a modifier key
            if (e.KeyPressEvent.Message == 256 && e.KeyPressEvent.VKey is 16 or 17 or 18) // Key down
            {
                switch (e.KeyPressEvent.VKey)
                {
                    case 16:
                        this.KeyStateList.Where(x => x.Device == originatingKeyboard)
                            .SetValue(v => v.ShiftPressed = true);
                        break;

                    case 17:
                        this.KeyStateList.Where(x => x.Device == originatingKeyboard)
                            .SetValue(v => v.ControlPressed = true);
                        break;

                    case 18:
                        this.KeyStateList.Where(x => x.Device == originatingKeyboard)
                            .SetValue(v => v.AltPressed = true);
                        break;
                }

                // With the modifier logged, get out
                return;
            }

            // Display the info
            richTextBox1.Text += e.KeyPressEvent.ToString();

            // Send the message to the window and then clean up
            if (e.KeyPressEvent.Message == 257 && e.KeyPressEvent.VKey is not 16 or 17 or 18)  // Key up
            {
                KeyboardProcessor.SendMessageToWindow(
                    SendKeyMapper.GetKeyCode(e.KeyPressEvent.VKeyName,  this.KeyStateList.FirstOrDefault(x => x.Device == originatingKeyboard)),
                    this.WatchedApps.FirstOrDefault(x => x.Keyboard == e.KeyPressEvent.TargetApp.Keyboard)!.WindowTitle);
                this.KeyStateList.Where(x => x.Device == originatingKeyboard).SetValue(v => v.AltPressed = false);
                this.KeyStateList.Where(x => x.Device == originatingKeyboard).SetValue(v => v.ControlPressed = false);
                this.KeyStateList.Where(x => x.Device == originatingKeyboard).SetValue(v => v.ShiftPressed = false);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Kill the Input Manager
            this.inputManager.KeyPressed -= InputManager_KeyPressed;
            this.inputManager.Close();
            Application.Exit();
        }
    }
}
