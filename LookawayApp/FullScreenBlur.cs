#nullable enable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LookawayApp
{
    public class FullScreenBlur : Form
    {
        private readonly Timer _blurTimer;
        private readonly Label _breakLabel;
        private readonly Button _skipButton;
        public event Action? BreakSkipped;

        public FullScreenBlur()
        {
            // Form settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.7;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);

            // Label setup
            _breakLabel = new Label
            {
                Text = "TIME TO TAKE A BREAK",
                ForeColor = ColorTranslator.FromHtml("#FF6B6B"),
                Font = new Font("Arial", 24, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Skip button setup
            _skipButton = new Button
            {
                Text = "Skip Break",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#FF8E53"),
                Size = new Size(120, 40)
            };
            _skipButton.Click += SkipButton_Click;

            // Add controls to form
            this.Controls.Add(_breakLabel);
            this.Controls.Add(_skipButton);

            // Timer for redrawing blur effect
            _blurTimer = new Timer { Interval = 10 };
            _blurTimer.Tick += BlurTimer_Tick;
            _blurTimer.Start();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _breakLabel.Location = new Point((this.Width - _breakLabel.Width) / 2, 
                                             (this.Height - _breakLabel.Height) / 2 - 50);
            _skipButton.Location = new Point((this.Width - _skipButton.Width) / 2, 
                                             (this.Height - _skipButton.Height) / 2 + 50);
        }

        public void ShowOverlay()
        {
            this.Show();
        }

        public void HideOverlay()
        {
            this.Hide();
        }

        private void SkipButton_Click(object? sender, EventArgs e)
        {
            this.Hide();
            BreakSkipped?.Invoke();
        }

        private void BlurTimer_Tick(object? sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                       Color.FromArgb(160, 0, 0, 20), Color.FromArgb(140, 40, 0, 0), 90f))
            {
                ColorBlend blend = new ColorBlend(3)
                {
                    Colors = new Color[] {
                        Color.FromArgb(160, 0, 0, 20),
                        Color.FromArgb(150, 20, 0, 10),
                        Color.FromArgb(140, 40, 0, 0)
                    },
                    Positions = new float[] { 0f, 0.5f, 1f }
                };
                brush.InterpolationColors = blend;
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }
    }
}

