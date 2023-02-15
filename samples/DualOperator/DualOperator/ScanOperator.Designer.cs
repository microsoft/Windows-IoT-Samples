namespace DualOperator
{
    partial class ScanOperator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.KeyboardName = new System.Windows.Forms.TextBox();
            this.ClearButton = new System.Windows.Forms.Button();
            this.App1Button = new System.Windows.Forms.Button();
            this.App2Button = new System.Windows.Forms.Button();
            this.ClipboardButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.Instructions = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // KeyboardName
            // 
            this.KeyboardName.Location = new System.Drawing.Point(12, 46);
            this.KeyboardName.Name = "KeyboardName";
            this.KeyboardName.Size = new System.Drawing.Size(611, 23);
            this.KeyboardName.TabIndex = 0;
            // 
            // ClearButton
            // 
            this.ClearButton.BackColor = System.Drawing.Color.Red;
            this.ClearButton.ForeColor = System.Drawing.Color.White;
            this.ClearButton.Location = new System.Drawing.Point(12, 92);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(75, 41);
            this.ClearButton.TabIndex = 1;
            this.ClearButton.Text = "Clear and run";
            this.ClearButton.UseVisualStyleBackColor = false;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // App1Button
            // 
            this.App1Button.BackColor = System.Drawing.Color.Blue;
            this.App1Button.ForeColor = System.Drawing.Color.White;
            this.App1Button.Location = new System.Drawing.Point(102, 92);
            this.App1Button.Name = "App1Button";
            this.App1Button.Size = new System.Drawing.Size(75, 41);
            this.App1Button.TabIndex = 2;
            this.App1Button.Text = "Set as App 1";
            this.App1Button.UseVisualStyleBackColor = false;
            this.App1Button.Click += new System.EventHandler(this.App1Button_Click);
            // 
            // App2Button
            // 
            this.App2Button.BackColor = System.Drawing.Color.Blue;
            this.App2Button.ForeColor = System.Drawing.Color.White;
            this.App2Button.Location = new System.Drawing.Point(193, 92);
            this.App2Button.Name = "App2Button";
            this.App2Button.Size = new System.Drawing.Size(75, 41);
            this.App2Button.TabIndex = 3;
            this.App2Button.Text = "Set as App 2";
            this.App2Button.UseVisualStyleBackColor = false;
            this.App2Button.Click += new System.EventHandler(this.App2Button_Click);
            // 
            // ClipboardButton
            // 
            this.ClipboardButton.BackColor = System.Drawing.Color.Blue;
            this.ClipboardButton.ForeColor = System.Drawing.Color.White;
            this.ClipboardButton.Location = new System.Drawing.Point(286, 92);
            this.ClipboardButton.Name = "ClipboardButton";
            this.ClipboardButton.Size = new System.Drawing.Size(75, 41);
            this.ClipboardButton.TabIndex = 4;
            this.ClipboardButton.Text = "Copy to Clipboard";
            this.ClipboardButton.UseVisualStyleBackColor = false;
            this.ClipboardButton.Click += new System.EventHandler(this.ClipboardButton_Click);
            // 
            // ExitButton
            // 
            this.ExitButton.BackColor = System.Drawing.Color.Red;
            this.ExitButton.ForeColor = System.Drawing.Color.White;
            this.ExitButton.Location = new System.Drawing.Point(383, 92);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(75, 41);
            this.ExitButton.TabIndex = 5;
            this.ExitButton.Text = "Exit Dual Operator";
            this.ExitButton.UseVisualStyleBackColor = false;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // Instructions
            // 
            this.Instructions.AutoSize = true;
            this.Instructions.Location = new System.Drawing.Point(12, 12);
            this.Instructions.Name = "Instructions";
            this.Instructions.Size = new System.Drawing.Size(325, 15);
            this.Instructions.TabIndex = 6;
            this.Instructions.Text = "Press any key on any keyboard to learn the keyboard name...";
            // 
            // ScanOperator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 161);
            this.Controls.Add(this.Instructions);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.ClipboardButton);
            this.Controls.Add(this.App2Button);
            this.Controls.Add(this.App1Button);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.KeyboardName);
            this.Name = "ScanOperator";
            this.Text = "Scan Operator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox KeyboardName;
        private Button ClearButton;
        private Button App1Button;
        private Button App2Button;
        private Button ClipboardButton;
        private Button ExitButton;
        private Label Instructions;
    }
}