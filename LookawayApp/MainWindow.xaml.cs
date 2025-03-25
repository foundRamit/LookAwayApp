#nullable enable
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
        private TimeSpan _currentTime;
        private TimeSpan _workDuration;
        private TimeSpan _breakDuration;
        private bool _isBreakTime = false;
        private FullScreenBlur? _blurScreen;

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _workDuration = TimeSpan.FromMinutes(1);  // Default work duration
            _breakDuration = TimeSpan.FromSeconds(15);  // Default break duration
            _currentTime = _workDuration;
            UpdateTimerDisplay(_workDuration);
        }

        private void SetTimer_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WorkHoursInput.Text, out int hours) &&
                int.TryParse(WorkMinutesInput.Text, out int minutes) &&
                int.TryParse(WorkSecondsInput.Text, out int seconds) &&
                (hours >= 0 || minutes >= 0 || seconds > 0))
            {
                _workDuration = new TimeSpan(hours, minutes, seconds);
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a valid work duration.");
                return;
            }

            if (int.TryParse(BreakMinutesInput.Text, out int breakMinutes) &&
                int.TryParse(BreakSecondsInput.Text, out int breakSeconds) &&
                (breakMinutes >= 0 || breakSeconds > 0))
            {
                _breakDuration = new TimeSpan(0, breakMinutes, breakSeconds);
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a valid break duration.");
                return;
            }

            _currentTime = _workDuration;
            UpdateTimerDisplay(_currentTime);
            System.Windows.MessageBox.Show("Timer settings updated!");
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTime.TotalSeconds > 0)
            {
                _timer.Start();
            }
            else
            {
                System.Windows.MessageBox.Show("Please set a valid timer before starting.");
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _isBreakTime = false;
            _currentTime = _workDuration;
            UpdateTimerDisplay(_currentTime);
            HideBlurOverlay();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Show notification before break
            if (_currentTime.TotalSeconds == 15 && !_isBreakTime)
            {
                ShowBreakNotification();
            }

            // Countdown logic
            if (_currentTime.TotalSeconds > 0)
            {
                _currentTime = _currentTime.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerDisplay(_currentTime);
            }
            else
            {
                _timer.Stop();
                if (!_isBreakTime)
                {
                    ShowBlurOverlay();  // Show blur screen when break starts
                    _currentTime = _breakDuration;
                    _isBreakTime = true;
                    _timer.Start();
                }
                else
                {
                    HideBlurOverlay();  // Hide blur screen when break ends
                    _currentTime = _workDuration;
                    _isBreakTime = false;
                    _timer.Start();
                }
            }
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

        private void ShowBlurOverlay()
        {
            if (_blurScreen == null)
            {
                _blurScreen = new FullScreenBlur();
                _blurScreen.BreakSkipped += RestartWorkTimer; // Attach event listener
            }

            _blurScreen.Show();
            _blurScreen.TopMost = true;  // Ensure it's on top of all other apps
            _blurScreen.ShowOverlay();
        }

        private void HideBlurOverlay()
        {
            if (_blurScreen != null)
            {
                _blurScreen.HideOverlay();
                _blurScreen.BreakSkipped -= RestartWorkTimer; // Detach event listener
                _blurScreen = null;  // Make sure to nullify after hiding
            }
        }

        private void RestartWorkTimer()
        {
            HideBlurOverlay();
            _currentTime = _workDuration;
            _isBreakTime = false;
            UpdateTimerDisplay(_currentTime);
            _timer.Start();
        }
    }
}
