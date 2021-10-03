using System;
using System.Configuration;
using Microsoft.Win32;

namespace TrayDesk
{
    public class UpDownTimer
    {
        private readonly int _heightThreshold;
        private readonly double _minUpShare;
        private readonly TimeSpan _reportingSpan;
        private readonly TimeSpan _daybreak;
        private readonly TimeSpan _dontWarnBefore;

        private DateTime _lastReport = DateTime.MinValue;
        private bool _lastDown;
        private bool _locked;

        public TimeSpan Up { get; private set; }
        public TimeSpan Down { get; private set; }

        public double UpShare => Up.TotalSeconds / (Down.TotalSeconds + Up.TotalSeconds);

        public bool ShowWarning => UpShare < _minUpShare && Up + Down > _dontWarnBefore && _lastDown;

        public UpDownTimer()
        {
            _heightThreshold = int.Parse(ConfigurationManager.AppSettings["HeightThreshold"]);
            _minUpShare = double.Parse(ConfigurationManager.AppSettings["MinUpShare"]);
            _reportingSpan = TimeSpan.Parse(ConfigurationManager.AppSettings["ReportingSpan"]);
            _daybreak = TimeSpan.Parse(ConfigurationManager.AppSettings["Daybreak"]);
            _dontWarnBefore = TimeSpan.Parse(ConfigurationManager.AppSettings["DontWarnBefore"]);

            // This even is fired whenever user locks/unlocks the PC
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        public void AddReport(int height)
        {
            if (_locked)
            {
                return;
            }

            var daybreak = DateTime.Today.Add(_daybreak);
            if (_lastReport < daybreak && DateTime.Now >= daybreak)
            {
                // it's new day, let's reset timers to zero
                Up = Down = TimeSpan.Zero;
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