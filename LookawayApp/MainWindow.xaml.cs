using System.Windows; // For WPF
using System.Windows.Controls; // For WPF controls
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Forms;
using System; // For EventArgs
 // For NotifyIcon


namespace LookawayApp
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer focusTimer;
        private DispatcherTimer restTimer;
        private int focusTime = 20 * 60; // 20 minutes
        private int restTime = 20; // 20 seconds
        private NotifyIcon trayIcon;
        private bool isPaused = false;

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public MainWindow()
        {
            InitializeComponent();
            System.Windows.MessageBox.Show("App started!");
            SetupTimers();
            SetupTrayIcon();
            this.Show();  
        }

        private void SetupTimers()
        {
            focusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            focusTimer.Tick += FocusTimer_Tick;
            focusTimer.Start();

            restTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            restTimer.Tick += RestTimer_Tick;
        }

        private void SetupTrayIcon()
        {
            try
            {
                trayIcon = new NotifyIcon();
                // trayIcon.Icon = new System.Drawing.Icon("icon.ico");
                trayIcon.Visible = true;

                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Start", null, (s, e) => StartFocusMode());
                contextMenu.Items.Add("Pause", null, (s, e) => TogglePause());
                contextMenu.Items.Add("Skip Rest", null, (s, e) => SkipRest());
                contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

                trayIcon.ContextMenuStrip = contextMenu;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Icon file not found! Ensure 'icon.ico' exists in the project directory.\n\nDetails: {ex.Message}");
            }
        }

        private void FocusTimer_Tick(object sender, EventArgs e)
        {
            if (IsUserIdle() || IsMeetingOrVideoActive()) return;

            focusTime--;
            UpdateUI("Focus Mode", focusTime);

            if (focusTime <= 0)
            {
                StartRestMode();
            }
        }

        private void StartRestMode()
        {
            focusTimer.Stop();
            restTimer.Start();
            UpdateUI("Rest Mode", restTime);
            System.Media.SystemSounds.Beep.Play();
        }

        private void RestTimer_Tick(object sender, EventArgs e)
        {
            restTime--;
            UpdateUI("Rest Mode", restTime);

            if (restTime <= 0)
            {
                ResetFocusMode();
            }
        }

        private void ResetFocusMode()
        {
            restTimer.Stop();
            focusTime = 20 * 60;
            focusTimer.Start();
            UpdateUI("Focus Mode", focusTime);
        }

        private void UpdateUI(string mode, int timeLeft)
        {
            ModeLabel.Content = mode;
            TimeLabel.Content = $"{timeLeft / 60:D2}:{timeLeft % 60:D2}";
        }

        private bool IsUserIdle()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(LASTINPUTINFO));
            GetLastInputInfo(ref lastInputInfo);
            uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
            return idleTime > 10000; // 10 seconds
        }

        private bool IsMeetingOrVideoActive()
        {
            string[] meetingProcesses = { "zoom", "teams", "skype", "discord" };
            string[] videoProcesses = { "vlc", "mpv", "netflix" };

            var runningProcesses = Process.GetProcesses().Select(p => p.ProcessName.ToLower()).ToList();

            return meetingProcesses.Any(p => runningProcesses.Contains(p)) || videoProcesses.Any(p => runningProcesses.Contains(p));
        }

        private void TogglePause()
        {
            if (isPaused)
            {
                focusTimer.Start();
                restTimer.Start();
                isPaused = false;
            }
            else
            {
                focusTimer.Stop();
                restTimer.Stop();
                isPaused = true;
            }
        }

        private void StartFocusMode()
        {
            focusTimer.Start();
        }

        private void SkipRest()
        {
            ResetFocusMode();
        }

        private void ExitApplication()
        {
            trayIcon.Visible = false;
            System.Windows.Application.Current.Shutdown();
        }
    }
}
