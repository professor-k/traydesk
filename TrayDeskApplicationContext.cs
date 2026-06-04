using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayDesk.Properties;

namespace TrayDesk
{
    public class TrayDeskApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly Timer _timer;
        private readonly ToolStripMenuItem _pauseMenuItem;
        private readonly ToolStripMenuItem _annoyingMenuItem;
        private readonly ToolStripMenuItem _startupMenuItem;
        private readonly UpDownTimer _upDownTimer;

        private readonly SerialPortParser _serialPortParser;

        public TrayDeskApplicationContext()
        {
            _upDownTimer = new UpDownTimer();
            _serialPortParser = new SerialPortParser();
            _serialPortParser.DataReceived += _serialPortParser_DataReceived;

            _pauseMenuItem = new ToolStripMenuItem("Pause", null, pauseToolStripMenuItem_Click);
            _annoyingMenuItem = new ToolStripMenuItem("Annoying mode", null, annoyingToolStripMenuItem_Click)
            {
                CheckOnClick = true,
                Checked = Settings.Default.Annoying,
            };
            _startupMenuItem = new ToolStripMenuItem("Run at startup", null, startupToolStripMenuItem_Click)
            {
                CheckOnClick = true,
                Checked = WindowsStartup.IsEnabled,
            };

            var strip = new ContextMenuStrip();
            strip.Items.Add(_pauseMenuItem);
            strip.Items.Add(new ToolStripMenuItem("Reset", null, resetToolStripMenuItem_Click));
            strip.Items.Add(_annoyingMenuItem);
            strip.Items.Add(_startupMenuItem);
            strip.Items.Add(new ToolStripMenuItem("Settings...", null, settingsToolStripMenuItem_Click));
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => Application.Exit()));

            _trayIcon = new NotifyIcon
            {
                ContextMenuStrip = strip,
                Visible = true,
            };

            _timer = new Timer
            {
                Enabled = true,
                Interval = 1000,
            };
            _timer.Tick += timer_Tick;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _upDownTimer.Pause = !_upDownTimer.Pause;
        }

        private void annoyingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // CheckOnClick has already toggled Checked by the time this fires; just persist it.
            Settings.Default.Annoying = _annoyingMenuItem.Checked;
            Settings.Default.Save();
        }

        private void startupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // CheckOnClick has already toggled Checked; mirror it into the registry Run entry.
            WindowsStartup.Set(_startupMenuItem.Checked);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Reset both counters to zero?", "TrayDesk",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _upDownTimer.Reset();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new SettingsForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                _upDownTimer.ReloadConfig();
            }
        }

        private void _serialPortParser_DataReceived(object sender, int e)
        {
            _upDownTimer.AddReport(e);
        }

        private bool _blink;
        private void timer_Tick(object sender, EventArgs e)
        {
            _serialPortParser.TryReopenIfNeeded();

            var iconType = IconBuilder.IconType.Default;
            if (!_serialPortParser.IsOpen || _upDownTimer.Pause || _upDownTimer.Locked)
            {
                iconType = IconBuilder.IconType.Disconnected;
            }
            else if (_upDownTimer.ShowWarning)
            {
                iconType = IconBuilder.IconType.Warn;
            }
            else if (_upDownTimer.ShowError)
            {
                iconType = IconBuilder.IconType.Error;
            }

            var isAlert = iconType is IconBuilder.IconType.Warn or IconBuilder.IconType.Error;

            if (_blink)
            {
                iconType |= IconBuilder.IconType.Blink;
            }

            // In "annoying mode" play the critical-stop sound in sync with the blink to make the
            // warning impossible to ignore (the gentler default beep reads as just a notification).
            if (_annoyingMenuItem.Checked && isAlert && _blink)
            {
                SystemSounds.Hand.Play();
            }

            var icon = IconBuilder.CreateIcon(_upDownTimer.Up, _upDownTimer.Down, iconType);
            _blink = !_blink;
            _trayIcon.Icon = icon;
            var availableDownTime = _upDownTimer.AvailableDownTime;
            var availableDownTimeTruncated = new TimeSpan(availableDownTime.Ticks - (availableDownTime.Ticks % 10000000));

            _pauseMenuItem.Text = _upDownTimer.Pause ? "Resume" : "Pause";

            if (!_serialPortParser.IsOpen)
            {
                _trayIcon.Text = "No sensor detected";
            }
            else if (_upDownTimer.Pause)
            {
                _trayIcon.Text = "On pause";
            }
            else
            {
                _trayIcon.Text = @$"Up: {_upDownTimer.Up}
Down: {_upDownTimer.Down}
Share: {_upDownTimer.UpShare:P}
Avail: {availableDownTimeTruncated}";
            }

            // Handle must be destroyed to prevent memory leakage
            DestroyIcon(icon.Handle);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _trayIcon?.Dispose();
                _timer?.Dispose();
                _serialPortParser?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}