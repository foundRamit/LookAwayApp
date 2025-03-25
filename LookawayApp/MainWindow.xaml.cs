using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms;
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

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            _reminderInterval = TimeSpan.FromMinutes(1);
            _reminderTimer = new DispatcherTimer { Interval = _reminderInterval };
            _reminderTimer.Tick += Reminder_Tick;

            _workDuration = TimeSpan.FromMinutes(1);
            _breakDuration = TimeSpan.FromSeconds(15);
            _currentTime = _workDuration;
            UpdateTimerDisplay(_workDuration);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            System.Windows.Application.Current.Shutdown(); // Ensures process termination
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTime.TotalSeconds > 0)
            {
                _timer.Start();
                _reminderTimer.Start();
                ShowFloatingCountdown();
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
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_currentTime.TotalSeconds == 15 && !_isBreakTime)
            {
                ShowBreakNotification();
            }

            if (_currentTime.TotalSeconds > 0)
            {
                _currentTime = _currentTime.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerDisplay(_currentTime);
                UpdateFloatingCountdown(_currentTime);
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
                }
                else
                {
                    HideBlurOverlay();
                    _currentTime = _workDuration;
                    _isBreakTime = false;
                    _timer.Start();
                    _reminderTimer.Start();
                }
            }
        }

        private void Reminder_Tick(object? sender, EventArgs e)
        {
            ShowReminderNotification("Blink Reminder", "Remember to blink and take a deep breath!");
        }

        private void UpdateTimerDisplay(TimeSpan time)
        {
            TimerDisplay.Text = time.ToString(@"hh\:mm\:ss");
        }

        private void ShowBreakNotification()
        {
            NotifyIcon notifyIcon = new NotifyIcon
            {
                BalloonTipTitle = "Upcoming Break",
                BalloonTipText = "Your break starts in 15 seconds!",
                Visible = true,
                Icon = SystemIcons.Information
            };
            notifyIcon.ShowBalloonTip(3000);
        }

        private void ShowReminderNotification(string title, string message)
        {
            NotifyIcon notifyIcon = new NotifyIcon
            {
                BalloonTipTitle = title,
                BalloonTipText = message,
                Visible = true,
                Icon = SystemIcons.Information
            };
            notifyIcon.ShowBalloonTip(5000);
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
            _floatingCountdown?.Hide(); // Use Hide() instead of Close() to prevent reinitialization issues
        }

        private void ShowBlurOverlay()
        {
            if (_blurScreen == null)
            {
                _blurScreen = new FullScreenBlur();
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
                (minutes > 0 || seconds > 0))  // Ensure non-zero input
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
