#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LookawayApp
{
    public class FullScreenBlur : Form
    {
        private readonly Timer _blurTimer;
        private readonly Label _breakLabel;
        private readonly Button _skipButton;
        public event Action? BreakSkipped; // Event to notify main app

        public FullScreenBlur()
        {
            // Form properties
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.7; // Semi-transparent background
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0); // Fill entire screen

            // Break label
            _breakLabel = new Label
            {
                Text = "TIME TO TAKE A BREAK",
                ForeColor = Color.White,
                Font = new Font("Arial", 24, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Skip button
            _skipButton = new Button
            {
                Text = "Skip Break",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Size = new Size(120, 40)
            };

            _skipButton.Click += SkipButton_Click;

            // Add controls to the form
            this.Controls.Add(_breakLabel);
            this.Controls.Add(_skipButton);

            // Timer to update blur effect
            _blurTimer = new Timer { Interval = 10 };
            _blurTimer.Tick += BlurTimer_Tick;
            _blurTimer.Start();
        }

        // Position label and button in the center
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _breakLabel.Location = new Point((this.Width - _breakLabel.Width) / 2, (this.Height - _breakLabel.Height) / 2 - 50);
            _skipButton.Location = new Point((this.Width - _skipButton.Width) / 2, (this.Height - _skipButton.Height) / 2 + 50);
        }

        // Show the overlay
        public void ShowOverlay()
        {
            this.Show();
        }

        // Hide the overlay
        public void HideOverlay()
        {
            this.Hide();
        }

        // Skip button action
        private void SkipButton_Click(object? sender, EventArgs e)
        {
            HideOverlay();
            BreakSkipped?.Invoke(); // Notify main app to restart work timer
        }

        // Timer tick for updating the blur effect
        private void BlurTimer_Tick(object? sender, EventArgs e)
        {
            Invalidate();
        }

        // Paint blur effect on form
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Brush brush = new SolidBrush(Color.FromArgb(120, 0, 0, 0))) // Semi-transparent black
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }
    }
}
