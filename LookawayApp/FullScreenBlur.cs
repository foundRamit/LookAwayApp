using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LookawayApp
{
    public class FullScreenBlur : Form
    {
        private readonly Timer _timer;

        public FullScreenBlur()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackColor = Color.Black;
            this.Opacity = 0.7;  // Semi-transparent background
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);  // Cover the entire screen

            // Enable double buffering to prevent flickering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            _timer = new Timer();
            _timer.Interval = 10;
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        // Show the blur effect
        public void ShowOverlay()
        {
            this.Show();
        }

        // Hide the blur effect
        public void HideOverlay()
        {
            this.Hide();
        }

        // Custom rendering to create a blur effect and centered text
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Create a semi-transparent blurred overlay
            using (Brush blurBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))  // Darker semi-transparent overlay
            {
                e.Graphics.FillRectangle(blurBrush, this.ClientRectangle);
            }

            // Draw centered text
            string breakText = "TIME TO TAKE A BREAK";
            Font textFont = new Font("Arial", 36, FontStyle.Bold);
            SizeF textSize = e.Graphics.MeasureString(breakText, textFont);

            // Center the text
            float textX = (this.ClientSize.Width - textSize.Width) / 2;
            float textY = (this.ClientSize.Height - textSize.Height) / 2;

            using (Brush textBrush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(breakText, textFont, textBrush, textX, textY);
            }
        }

        // Periodically update the blur effect
        private void Timer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
