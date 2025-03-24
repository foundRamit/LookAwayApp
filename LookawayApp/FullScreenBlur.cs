using System;
using System.Drawing;
using System.Windows.Forms;

namespace LookawayApp
{
    public class FullScreenBlur : Form
    {
        public FullScreenBlur()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.Opacity = 0.7; // Semi-transparent blur effect
            this.TopMost = true;
            this.ShowInTaskbar = false;
        }

        public void ShowOverlay()
        {
            this.Invoke(new Action(() =>
            {
                this.Show();
            }));
        }

        public void HideOverlay()
        {
            this.Invoke(new Action(() =>
            {
                this.Hide();
            }));
        }
    }
}
