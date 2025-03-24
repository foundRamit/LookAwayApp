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

        public MainWindow()
        {
            InitializeComponent();
            _timeLeft = TimeSpan.FromMinutes(20); // Default 20-minute timer
            UpdateTimerDisplay();
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
            _breakTimer = new Timer(20000); // 20 seconds for the break
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

