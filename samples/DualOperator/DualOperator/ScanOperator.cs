using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using DualOperator.Helpers;

namespace DualOperator
{
    public partial class ScanOperator : Form
    {
        private readonly RawInput inputManager;

        public ScanOperator()
        {
            InitializeComponent();

            // Initialize the Input Manager
            this.inputManager = new RawInput(this.Handle, false);
            this.inputManager.KeyPressed += InputManager_KeyPressed;

            // Disable the buttons until after a key has been pressed
            this.App1Button.Enabled = false;
            this.App2Button.Enabled = false;
            this.ClipboardButton.Enabled = false;
            this.ClearButton.Enabled = false;
        }

        private void InputManager_KeyPressed(object sender, RawInputEventArg e)
        {
            // Only react to key up events
            if (e.KeyPressEvent.Message != 257)
            {
                return;
            }

            // Set the keyboard name in the textbox
            this.KeyboardName.Text = e.KeyPressEvent.DeviceName.Replace(@"\", @"\\");

            // And enable the buttons
            this.App1Button.Enabled = true;
            this.App2Button.Enabled = true;
            this.ClipboardButton.Enabled = true;
            this.ClearButton.Enabled = true;
        }

        private void App1Button_Click(object sender, EventArgs e)
        {
            // Load the current OPERATORS file and change the first keyboard item
            LoadOperator.LoadApps();
            if (LoadOperator.AppList is {Count: > 0})
            {
                LoadOperator.AppList[0].Keyboard = this.KeyboardName.Text;
                File.WriteAllText("operators.json", JsonSerializer.Serialize(LoadOperator.AppList));
            }
        }

        private void App2Button_Click(object sender, EventArgs e)
        {
            // Load the current OPERATORS file and change the second keyboard item
            LoadOperator.LoadApps();
            if (LoadOperator.AppList is { Count: > 1 })
            {
                LoadOperator.AppList[1].Keyboard = this.KeyboardName.Text;
                File.WriteAllText("operators.json", JsonSerializer.Serialize(LoadOperator.AppList));
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            // Reset to get the next keyboard run
            this.KeyboardName.Text = string.Empty;
            this.App1Button.Enabled = false;
            this.App2Button.Enabled = false;
            this.ClipboardButton.Enabled = false;
            this.ClearButton.Enabled = false;
        }

        private void ClipboardButton_Click(object sender, EventArgs e)
        {
            // Copy the text to the clipboard
            Clipboard.SetText(this.KeyboardName.Text);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            // Kill the Input Manager
            this.inputManager.KeyPressed -= InputManager_KeyPressed;
            this.inputManager.Close();
            Application.Exit();

        }
    }
}
