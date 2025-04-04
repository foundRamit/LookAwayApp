using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Forms; // For system tray support
using System.Drawing;

namespace LookawayApp
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private DispatcherTimer _reminderTimer;
        private TimeSpan _currentTime;
        private TimeSpan _workDuration;
        private TimeSpan _breakDuration;
        private TimeSpan _reminderInterval;
        private bool _isBreakTime = false;
        private FloatingCountdown? _floatingCountdown;
        private FullScreenBlur? _blurScreen;
        private SystemTray? _systemTray;
        private bool _closeToTray = true;

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            _reminderInterval = TimeSpan.FromMinutes(1);
            _reminderTimer = new DispatcherTimer { Interval = _reminderInterval };
            _reminderTimer.Tick += Reminder_Tick;

            _workDuration = TimeSpan.FromMinutes(20);
            _breakDuration = TimeSpan.FromSeconds(20);
            _currentTime = _workDuration;
            UpdateTimerDisplay(_currentTime);

            InitializeSystemTray();
            Closing += OnWindowClosing;
        }

        private void InitializeSystemTray()
        {
            _systemTray = new SystemTray(this);
            _systemTray.StartTimer += (s, e) => StartButton_Click(s, new RoutedEventArgs());
            _systemTray.StopTimer += (s, e) => StopButton_Click(s, new RoutedEventArgs());
            _systemTray.SkipBreak += (s, e) => SkipBreak();
            _systemTray.ShowSettings += (s, e) => ShowSettings();
        }

        public void ShowAndActivate()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Show();
            Activate();
            Focus();
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (_closeToTray)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset all tab buttons to inactive style
            QuickStartTab.Style = (Style)FindResource("TabButtonStyle");
            RoutinesTab.Style = (Style)FindResource("TabButtonStyle");
            SettingsTab.Style = (Style)FindResource("TabButtonStyle");
            
            // Hide all content
            QuickStartContent.Visibility = Visibility.Collapsed;
            RoutinesContent.Visibility = Visibility.Collapsed;
            SettingsContent.Visibility = Visibility.Collapsed;
            
            // Set active tab and show content; using fully-qualified type for Button:
            System.Windows.Controls.Button clickedButton = (System.Windows.Controls.Button)sender;
            clickedButton.Style = (Style)FindResource("ActiveTabButtonStyle");
            
            if (clickedButton == QuickStartTab)
                QuickStartContent.Visibility = Visibility.Visible;
            else if (clickedButton == RoutinesTab)
                RoutinesContent.Visibility = Visibility.Visible;
            else if (clickedButton == SettingsTab)
                SettingsContent.Visibility = Visibility.Visible;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTime.TotalSeconds > 0)
            {
                _timer.Start();
                _reminderTimer.Start();
                ShowFloatingCountdown();
                _systemTray?.UpdateTooltip($"Lookaway - {(_isBreakTime ? "Break" : "Focus")}: {_currentTime:hh\\:mm\\:ss}");
                _systemTray?.ShowNotification("Timer Started", $"Your {(_isBreakTime ? "break" : "focus")} timer has started");
            }
            else
            {
                System.Windows.MessageBox.Show("Please set a valid timer before starting.");
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _reminderTimer.Stop();
            _isBreakTime = false;
            _currentTime = _workDuration;
            UpdateTimerDisplay(_currentTime);
            HideBlurOverlay();
            HideFloatingCountdown();
            _systemTray?.UpdateTooltip("Lookaway - Timer stopped");
            _systemTray?.ShowNotification("Timer Stopped", "Your timer has been stopped");
        }

        private void SkipBreak()
        {
            if (_isBreakTime)
            {
                HideBlurOverlay();
                _currentTime = _workDuration;
                _isBreakTime = false;
                UpdateTimerDisplay(_currentTime);
                _timer.Start();
                _reminderTimer.Start();
                _systemTray?.ShowNotification("Break Skipped", "Starting a new focus session");
            }
        }

        private void ShowSettings()
        {
            SettingsWindow settingsWindow = new SettingsWindow { Owner = this };
            if (settingsWindow.ShowDialog() == true)
                LoadSettings();
        }

        private void LoadSettings()
        {
            // Placeholder for loading from Properties.Settings.Default.*
        }

        public void SetTimerValues(int hours, int minutes, int seconds, int breakMinutes, int breakSeconds)
        {
            WorkHours.Text = hours.ToString("00");
            WorkMinutes.Text = minutes.ToString("00");
            WorkSeconds.Text = seconds.ToString("00");

            BreakMinutes.Text = breakMinutes.ToString("00");
            BreakSeconds.Text = breakSeconds.ToString("00");

            SetTimer_Click(this, new RoutedEventArgs());
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_currentTime.TotalSeconds == 15 && !_isBreakTime)
                ShowBreakNotification();

            if (_currentTime.TotalSeconds > 0)
            {
                _currentTime = _currentTime.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerDisplay(_currentTime);
                UpdateFloatingCountdown(_currentTime);
                _systemTray?.UpdateTooltip($"Lookaway - {(_isBreakTime ? "Break" : "Focus")}: {_currentTime:mm\\:ss}");
            }
            else
            {
                _timer.Stop();
                _reminderTimer.Stop();
                if (!_isBreakTime)
                {
                    ShowBlurOverlay();
                    _currentTime = _breakDuration;
                    _isBreakTime = true;
                    _timer.Start();
                    _systemTray?.ShowNotification("Break Time", "Time to look away and rest your eyes!");
                }
                else
                {
                    HideBlurOverlay();
                    _currentTime = _workDuration;
                    _isBreakTime = false;
                    _timer.Start();
                    _reminderTimer.Start();
                    _systemTray?.ShowNotification("Focus Time", "Break is over. Back to work!");
                }
            }
        }

        private void Reminder_Tick(object? sender, EventArgs e)
        {
            _systemTray?.ShowNotification("Blink Reminder", "Remember to blink and take a deep breath!");
        }

        private void UpdateTimerDisplay(TimeSpan time)
        {
            TimerDisplay.Text = time.ToString(@"hh\:mm\:ss");
        }

        private void ShowBreakNotification()
        {
            _systemTray?.ShowNotification("Upcoming Break", "Your break starts in 15 seconds!");
        }

        private void ShowFloatingCountdown()
        {
            if (_floatingCountdown == null)
            {
                _floatingCountdown = new FloatingCountdown();
                _floatingCountdown.Show();
            }
        }

        private void UpdateFloatingCountdown(TimeSpan timeLeft)
        {
            _floatingCountdown?.UpdateCountdown((int)timeLeft.TotalSeconds);
        }

        private void HideFloatingCountdown()
        {
            _floatingCountdown?.Hide();
        }

        private void ShowBlurOverlay()
        {
            if (_blurScreen == null)
            {
                _blurScreen = new FullScreenBlur();
                _blurScreen.BreakSkipped += () => SkipBreak();
                _blurScreen.Show();
            }
        }

        private void HideBlurOverlay()
        {
            _blurScreen?.Close();
            _blurScreen = null;
        }

        private void SetTimer_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WorkHours.Text, out int hours) &&
                int.TryParse(WorkMinutes.Text, out int minutes) &&
                int.TryParse(WorkSeconds.Text, out int seconds) &&
                int.TryParse(BreakMinutes.Text, out int breakMinutes) &&
                int.TryParse(BreakSeconds.Text, out int breakSeconds))
            {
                _workDuration = new TimeSpan(hours, minutes, seconds);
                _breakDuration = new TimeSpan(0, breakMinutes, breakSeconds);
                _currentTime = _workDuration;
                UpdateTimerDisplay(_currentTime);
                System.Windows.MessageBox.Show("Timer updated!");
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid input. Please enter valid numbers.");
            }
        }

        private void SetReminder_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ReminderMinutes.Text, out int minutes) &&
                int.TryParse(ReminderSeconds.Text, out int seconds) &&
                (minutes > 0 || seconds > 0))
            {
                _reminderInterval = new TimeSpan(0, minutes, seconds);
                _reminderTimer.Interval = _reminderInterval;
                _reminderTimer.Start();
                System.Windows.MessageBox.Show($"Reminder set to {minutes} min {seconds} sec!");
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid input! Please enter valid minutes and seconds.");
            }
        }
    }
}

