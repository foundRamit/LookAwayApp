using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LookawayApp
{
    public partial class FloatingCountdown : Window
    {
        private DispatcherTimer _countdownTimer;
        private int _secondsLeft = 0;

        public FloatingCountdown()
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = false;

            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        public void StartCountdown(int seconds)
        {
            if (_countdownTimer.IsEnabled)
                _countdownTimer.Stop();

            _secondsLeft = seconds;
            UpdateCountdown(_secondsLeft);
            _countdownTimer.Start();
            Show();
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            if (_secondsLeft > 0)
            {
                _secondsLeft--;
                UpdateCountdown(_secondsLeft);
            }
            else
            {
                _countdownTimer.Stop();
                Hide();
            }
        }

        public void UpdateCountdown(int secondsLeft)
        {
            CountdownLabel.Text = TimeSpan.FromSeconds(secondsLeft).ToString(@"mm\:ss");
            // If CountdownLabel is a Label, use:
            // CountdownLabel.Content = ...
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
