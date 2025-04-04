using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows; // For Application.Current.Shutdown

namespace LookawayApp
{
    public class SystemTray : IDisposable
    {
        private NotifyIcon _notifyIcon = null!; // Use null! to tell compiler we'll initialize it
        private ContextMenuStrip _contextMenu = null!; // Use null! to tell compiler we'll initialize it
        private readonly MainWindow _mainWindow;
        private readonly Icon _appIcon;

        public event EventHandler? StartTimer;
        public event EventHandler? StopTimer;
        public event EventHandler? SkipBreak;
        public event EventHandler? ShowSettings;

        public SystemTray(MainWindow mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _appIcon = SystemIcons.Application;
            InitializeContextMenu();
            InitializeNotifyIcon();
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();
            var titleItem = new ToolStripMenuItem("Lookaway App")
            {
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#FF6B6B")
            };
            titleItem.Click += (s, e) => _mainWindow.ShowAndActivate();
            _contextMenu.Items.Add(titleItem);
            _contextMenu.Items.Add(new ToolStripSeparator());

            // Quick Start Items
            var quickStartItem = new ToolStripMenuItem("Quick Start")
            {
                Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Regular),
                ForeColor = ColorTranslator.FromHtml("#FF8E53")
            };
            _contextMenu.Items.Add(quickStartItem);

            var rule2020Item = new ToolStripMenuItem("20-20-20 Rule")
            {
                Image = SystemIcons.Information.ToBitmap()
            };
            rule2020Item.Click += (s, e) =>
            {
                _mainWindow.SetTimerValues(0, 20, 0, 0, 20);
                StartTimer?.Invoke(this, EventArgs.Empty);
            };
            quickStartItem.DropDownItems.Add(rule2020Item);

            var codingItem = new ToolStripMenuItem("Coding Session")
            {
                Image = SystemIcons.Application.ToBitmap()
            };
            codingItem.Click += (s, e) =>
            {
                _mainWindow.SetTimerValues(0, 45, 0, 5, 0);
                StartTimer?.Invoke(this, EventArgs.Empty);
            };
            quickStartItem.DropDownItems.Add(codingItem);

            var readingItem = new ToolStripMenuItem("Reading Mode")
            {
                Image = SystemIcons.Information.ToBitmap()
            };
            readingItem.Click += (s, e) =>
            {
                _mainWindow.SetTimerValues(0, 30, 0, 3, 0);
                StartTimer?.Invoke(this, EventArgs.Empty);
            };
            quickStartItem.DropDownItems.Add(readingItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var startItem = new ToolStripMenuItem("Start Timer")
            {
                Image = SystemIcons.Information.ToBitmap()
            };
            startItem.Click += (s, e) => StartTimer?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(startItem);

            var stopItem = new ToolStripMenuItem("Stop Timer")
            {
                Image = SystemIcons.Error.ToBitmap()
            };
            stopItem.Click += (s, e) => StopTimer?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(stopItem);

            var skipItem = new ToolStripMenuItem("Skip Break")
            {
                Image = SystemIcons.Warning.ToBitmap()
            };
            skipItem.Click += (s, e) => SkipBreak?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(skipItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var settingsItem = new ToolStripMenuItem("Settings")
            {
                Image = SystemIcons.Shield.ToBitmap()
            };
            settingsItem.Click += (s, e) => ShowSettings?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(settingsItem);

            var exitItem = new ToolStripMenuItem("Exit")
            {
                Image = SystemIcons.Hand.ToBitmap()
            };
            exitItem.Click += (s, e) => System.Windows.Application.Current.Shutdown();

            _contextMenu.Items.Add(exitItem);
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = _appIcon,
                Text = "Lookaway App - Eye Care Timer",
                Visible = true,
                ContextMenuStrip = _contextMenu
            };
            _notifyIcon.DoubleClick += (s, e) => _mainWindow.ShowAndActivate();
        }

        public void ShowNotification(string title, string message, int duration = 3000)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(duration);
        }

        public void UpdateTooltip(string text)
        {
            _notifyIcon.Text = text;
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
    }
}