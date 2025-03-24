using System;
using System.Timers;
using System.Windows;

namespace LookawayApp
{
    public partial class MainWindow : Window
    {
        private Timer _timer;
        private TimeSpan _timeLeft;
        private Timer _breakTimer;
        private TimeSpan _breakDuration;

        public MainWindow()
        {
            InitializeComponent();
            _timeLeft = TimeSpan.FromMinutes(20); // Default 20-minute timer
            _breakDuration = TimeSpan.FromSeconds(20); // Default 20 seconds break
            UpdateTimerDisplay();
        }

        private void SetTimer_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WorkHoursInput.Text, out int hours) &&
                int.TryParse(WorkMinutesInput.Text, out int minutes) &&
                int.TryParse(WorkSecondsInput.Text, out int seconds) &&
                (hours >= 0 || minutes > 0 || seconds > 0))
            {
                _timeLeft = new TimeSpan(hours, minutes, seconds);
            }
            else
            {
                MessageBox.Show("Please enter a valid work duration.");
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
                MessageBox.Show("Please enter a valid break duration.");
                return;
            }

            UpdateTimerDisplay();
            MessageBox.Show("Timer settings updated!");
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new Timer(1000); // 1-second interval
                _timer.Elapsed += Timer_Elapsed;
                _timer.Start();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            _timer = null;
            _timeLeft = TimeSpan.FromMinutes(20); // Reset timer
            UpdateTimerDisplay();
            HideBlurOverlay();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerDisplay();

                if (_timeLeft.TotalSeconds <= 0)
                {
                    _timer.Stop();
                    _timer = null;
                    ShowBlurOverlay();
                    StartBreakTimer();
                }
            });
        }

        private void UpdateTimerDisplay()
        {
            TimerDisplay.Text = _timeLeft.ToString(@"hh\:mm\:ss");
        }

        private void ShowBlurOverlay()
        {
            BlurOverlay.Visibility = Visibility.Visible;
            BreakMessage.Visibility = Visibility.Visible;
        }

        private void HideBlurOverlay()
        {
            BlurOverlay.Visibility = Visibility.Collapsed;
            BreakMessage.Visibility = Visibility.Collapsed;
        }

        private void StartBreakTimer()
        {
            _breakTimer = new Timer(_breakDuration.TotalMilliseconds);
            _breakTimer.Elapsed += BreakTimer_Elapsed;
            _breakTimer.Start();
        }

        private void BreakTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _breakTimer.Stop();
                _breakTimer = null;
                HideBlurOverlay();
                _timeLeft = TimeSpan.FromMinutes(20); // Reset timer
                UpdateTimerDisplay();
            });
        }
    }
}