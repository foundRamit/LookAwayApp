using System.Windows;

namespace LookawayApp
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            FocusTimeTextBox.Text = Properties.Settings.Default.FocusTime.ToString();
            RestTimeTextBox.Text = Properties.Settings.Default.RestTime.ToString();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FocusTime = int.Parse(FocusTimeTextBox.Text);
            Properties.Settings.Default.RestTime = int.Parse(RestTimeTextBox.Text);
            Properties.Settings.Default.Save();
            Close();
        }
    }
}
