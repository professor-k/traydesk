using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrayDesk
{
    public class TrayDeskApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly Timer _timer;
        private readonly UpDownTimer _upDownTimer;

        private readonly SerialPortParser _serialPortParser;

        public TrayDeskApplicationContext()
        {
            _upDownTimer = new UpDownTimer();
            _serialPortParser = new SerialPortParser(ConfigurationManager.AppSettings["Port"]);
            _serialPortParser.DataReceived += _serialPortParser_DataReceived;

            var strip = new ContextMenuStrip();
            strip.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => Application.Exit()));

            _trayIcon = new NotifyIcon
            {
                ContextMenuStrip = strip,
                Visible = true,
            };

            _timer = new Timer();
            _timer.Enabled = true;
            _timer.Interval = 1000;
            _timer.Tick += timer_Tick;
        }

        private void _serialPortParser_DataReceived(object sender, int e)
        {
            _upDownTimer.AddReport(e);
        }

        private bool _blink;
        private void timer_Tick(object sender, EventArgs e)
        {
            var icon = IconBuilder.CreateIcon(_upDownTimer.Up, _upDownTimer.Down, _upDownTimer.ShowWarning && _blink);
            _blink = !_blink;
            _trayIcon.Icon = icon;
            _trayIcon.Text = $"Up: {_upDownTimer.Up}\r\nDown: {_upDownTimer.Down}\r\nUp share: {_upDownTimer.UpShare:P}";

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