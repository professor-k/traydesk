using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrayDesk
{
    public class TrayDeskApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly Timer _timer;
        private readonly ToolStripMenuItem _pauseMenuItem;
        private readonly UpDownTimer _upDownTimer;

        private readonly SerialPortParser _serialPortParser;

        public TrayDeskApplicationContext()
        {
            _upDownTimer = new UpDownTimer();
            _serialPortParser = new SerialPortParser();
            _serialPortParser.DataReceived += _serialPortParser_DataReceived;

            _pauseMenuItem = new ToolStripMenuItem("Pause", null, pauseToolStripMenuItem_Click);
            var strip = new ContextMenuStrip();
            strip.Items.Add(_pauseMenuItem);
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

        private void _serialPortParser_DataReceived(object sender, int e)
        {
            _upDownTimer.AddReport(e);
        }

        private bool _blink;
        private void timer_Tick(object sender, EventArgs e)
        {
            _serialPortParser.TryReopenIfNeeded();

            var icon = IconBuilder.CreateIcon(_upDownTimer.Up, _upDownTimer.Down, _upDownTimer.ShowWarning && _blink, _serialPortParser.IsOpen && !_upDownTimer.Pause);
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