using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace LookawayApp
{
    public partial class SettingsWindow : Window
    {
        private const string SettingsFile = "settings.json";

        public int FocusTime { get; private set; }
        public int RestTime { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settings = JsonConvert.DeserializeObject<UserSettings>(json);

                if (settings != null)
                {
                    FocusTimeTextBox.Text = settings.FocusTime.ToString();
                    RestTimeTextBox.Text = settings.RestTime.ToString();
                }
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(FocusTimeTextBox.Text, out int focusTime) &&
                int.TryParse(RestTimeTextBox.Text, out int restTime) &&
                focusTime > 0 && restTime > 0)
            {
                var settings = new UserSettings
                {
                    FocusTime = focusTime,
                    RestTime = restTime
                };

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);

                FocusTime = focusTime;
                RestTime = restTime;

                this.DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter valid positive numbers.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        public class UserSettings
        {
            public int FocusTime { get; set; }
            public int RestTime { get; set; }
        }
    }
}
