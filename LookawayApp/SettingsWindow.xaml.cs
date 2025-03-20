using System;
using System.Windows;

namespace LookawayClone
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            FocusTimeTextBox.Text = Properties.Settings.Default.FocusTime.ToString();
            RestTimeTextBox.Text = Properties.Settings.Default.RestTime.ToString();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int focusTime = int.Parse(FocusTimeTextBox.Text);
                int restTime = int.Parse(RestTimeTextBox.Text);

                if (focusTime <= 0 || restTime <= 0)
                {
                    MessageBox.Show("Time values must be positive numbers.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Properties.Settings.Default.FocusTime = focusTime;
                Properties.Settings.Default.RestTime = restTime;
                Properties.Settings.Default.Save();
                
                this.DialogResult = true;
                Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numbers for time values.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}