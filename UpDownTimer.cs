using System;
using Microsoft.Win32;
using TrayDesk.Properties;

namespace TrayDesk
{
    public class UpDownTimer
    {
        private int _heightThreshold;
        private double _minUpShare;
        private readonly TimeSpan _reportingSpan;
        private TimeSpan _daybreak;
        private TimeSpan _dontWarnBefore;

        private DateTime _lastReport;
        private bool _lastDown;

        public TimeSpan Up { get; private set; }
        public TimeSpan Down { get; private set; }
        public bool Pause { get; set; }

        /// <summary>True while the PC is locked. Like Pause, time isn't counted and no alerts fire.</summary>
        public bool Locked { get; private set; }

        public double UpShare => Up.TotalSeconds / (Down.TotalSeconds + Up.TotalSeconds);

        public TimeSpan AvailableDownTime => (1 / _minUpShare - 1) * Up - Down;

        public bool ShowWarning => UpShare < _minUpShare && Up + Down > _dontWarnBefore && _lastDown;

        /// <summary>
        /// We didn't get any message in five seconds, sensor must be malfunctioning
        /// </summary>
        public bool ShowError => _lastReport.AddSeconds(5) < DateTime.Now;

        public UpDownTimer()
        {
            // general configuration
            _reportingSpan = Settings.Default.ReportingSpan;
            LoadConfig();

            // last state
            Down = Settings.Default.Down;
            Up = Settings.Default.Up;
            _lastReport = Settings.Default.LastReport;

            ResetIfDaybreak();

            // To prevent "error" state before app start and the first report received
            _lastReport = DateTime.Now;

            // This even is fired whenever user locks/unlocks the PC
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void LoadConfig()
        {
            _heightThreshold = Settings.Default.HeightThreshold;
            _minUpShare = Settings.Default.MinUpShare;
            _daybreak = Settings.Default.Daybreak;
            _dontWarnBefore = Settings.Default.DontWarnBefore;
        }

        /// <summary>
        /// Re-read the user-configurable settings, e.g. after they were changed in the settings window.
        /// </summary>
        public void ReloadConfig() => LoadConfig();

        /// <summary>
        /// Zero both counters (and persist), e.g. from the tray "Reset" menu.
        /// </summary>
        public void Reset()
        {
            Up = Down = TimeSpan.Zero;
            Settings.Default.Up = Up;
            Settings.Default.Down = Down;
            Settings.Default.Save();
        }

        public void AddReport(int height)
        {
            ResetIfDaybreak();

            if (Locked || Pause)
            {
                return;
            }

            _lastReport = DateTime.Now;
            if (height > _heightThreshold)
            {
                _lastDown = false;
                Up += _reportingSpan;
            }
            else
            {
                _lastDown = true;
                Down += _reportingSpan;
            }

            // Just save once in a minute, that should be enough
            if (_lastReport.Second == 0)
            {
                Settings.Default.Down = Down;
                Settings.Default.Up = Up;
                Settings.Default.LastReport = _lastReport;
                Settings.Default.Save();
            }
        }

        private void ResetIfDaybreak()
        {
            var daybreak = DateTime.Today.Add(_daybreak);
            if (_lastReport < daybreak && DateTime.Now >= daybreak)
            {
                // it's new day, let's reset timers to zero
                Up = Down = TimeSpan.Zero;
                // also unpause if paused
                Pause = false;
            }
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                Locked = true;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Locked = false;
            }
        }
    }
}