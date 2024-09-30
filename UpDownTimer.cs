using System;
using Microsoft.Win32;
using TrayDesk.Properties;

namespace TrayDesk
{
    public class UpDownTimer
    {
        private readonly int _heightThreshold;
        private readonly double _minUpShare;
        private readonly TimeSpan _reportingSpan;
        private readonly TimeSpan _daybreak;
        private readonly TimeSpan _dontWarnBefore;

        private DateTime _lastReport;
        private bool _lastDown;
        private bool _locked;

        public TimeSpan Up { get; private set; }
        public TimeSpan Down { get; private set; }
        public bool Pause { get; set; }

        public double UpShare => Up.TotalSeconds / (Down.TotalSeconds + Up.TotalSeconds);

        public TimeSpan AvailableDownTime => (1 / _minUpShare - 1) * Up - Down;

        public bool ShowWarning => UpShare < _minUpShare && Up + Down > _dontWarnBefore && _lastDown;

        public UpDownTimer()
        {
            // general configuration
            _heightThreshold = Settings.Default.HeightThreshold;
            _minUpShare = Settings.Default.MinUpShare;
            _reportingSpan = Settings.Default.ReportingSpan;
            _daybreak = Settings.Default.Daybreak;
            _dontWarnBefore = Settings.Default.DontWarnBefore;

            // last state
            Down = Settings.Default.Down;
            Up = Settings.Default.Up;
            _lastReport = Settings.Default.LastReport;

            ResetIfDaybreak();

            // This even is fired whenever user locks/unlocks the PC
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        public void AddReport(int height)
        {
            ResetIfDaybreak();

            if (_locked || Pause)
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
                _locked = true;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                _locked = false;
            }
        }
    }
}