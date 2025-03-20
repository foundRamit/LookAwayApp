using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

namespace LookawayClone
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer focusTimer;
        private DispatcherTimer restTimer;
        private int focusTime;
        private int restTime;
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
            LoadSettings();
            SetupTimers();
            SetupTrayIcon();
            this.Closing += MainWindow_Closing;
        }

        private void LoadSettings()
        {
            focusTime = Properties.Settings.Default.FocusTime;
            restTime = Properties.Settings.Default.RestTime;
            UpdateUI("Focus Mode", focusTime);
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
                trayIcon.Icon = System.Drawing.SystemIcons.Application; // Use system icon
                trayIcon.Visible = true;
                trayIcon.Text = "Lookaway Clone";
                trayIcon.DoubleClick += TrayIcon_DoubleClick;

                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Show", null, (s, e) => ShowWindow());
                contextMenu.Items.Add("Start", null, (s, e) => StartFocusMode());
                contextMenu.Items.Add(isPaused ? "Resume" : "Pause", null, (s, e) => TogglePause());
                contextMenu.Items.Add("Skip Rest", null, (s, e) => SkipRest());
                contextMenu.Items.Add("Settings", null, (s, e) => OpenSettings());
                contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

                trayIcon.ContextMenuStrip = contextMenu;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error setting up tray icon: {ex.Message}");
            }
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
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
            restTime = Properties.Settings.Default.RestTime;
            UpdateUI("Rest Mode", restTime);
            ShowWindow(); // Force window to show for rest reminder
            System.Media.SystemSounds.Exclamation.Play();
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
            focusTime = Properties.Settings.Default.FocusTime;
            focusTimer.Start();
            UpdateUI("Focus Mode", focusTime);
        }

        private void UpdateUI(string mode, int timeLeft)
        {
            ModeLabel.Content = mode;
            TimeLabel.Content = $"{timeLeft / 60:D2}:{timeLeft % 60:D2}";
            
            // Update tray icon tooltip
            if (trayIcon != null)
            {
                trayIcon.Text = $"Lookaway Clone - {mode} - {timeLeft / 60:D2}:{timeLeft % 60:D2}";
            }
        }

        private bool IsUserIdle()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
            GetLastInputInfo(ref lastInputInfo);
            uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
            return idleTime > 10000; // 10 seconds
        }

        private bool IsMeetingOrVideoActive()
        {
            string[] meetingProcesses = { "zoom", "teams", "skype", "discord" };
            string[] videoProcesses = { "vlc", "mpv", "netflix", "wmplayer", "chrome", "firefox", "msedge" };

            var runningProcesses = Process.GetProcesses().Select(p => p.ProcessName.ToLower()).ToList();

            return meetingProcesses.Any(p => runningProcesses.Contains(p)) || videoProcesses.Any(p => runningProcesses.Contains(p));
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                focusTimer.Stop();
                restTimer.Stop();
                UpdateUI(ModeLabel.Content.ToString() + " (Paused)", 
                         ModeLabel.Content.ToString().Contains("Focus") ? focusTime : restTime);
            }
            else
            {
                if (ModeLabel.Content.ToString().Contains("Focus"))
                    focusTimer.Start();
                else
                    restTimer.Start();
                
                UpdateUI(ModeLabel.Content.ToString().Replace(" (Paused)", ""), 
                         ModeLabel.Content.ToString().Contains("Focus") ? focusTime : restTime);
            }
            
            // Update menu item text
            if (trayIcon?.ContextMenuStrip?.Items.Count > 2)
            {
                trayIcon.ContextMenuStrip.Items[2].Text = isPaused ? "Resume" : "Pause";
            }
        }

        private void StartFocusMode()
        {
            if (restTimer.IsEnabled)
            {
                restTimer.Stop();
            }
            
            focusTime = Properties.Settings.Default.FocusTime;
            UpdateUI("Focus Mode", focusTime);
            focusTimer.Start();
            isPaused = false;
        }

        private void SkipRest()
        {
            if (restTimer.IsEnabled)
            {
                ResetFocusMode();
            }
        }

        private void OpenSettings()
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
            
            // Reload settings
            LoadSettings();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void ExitApplication()
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
