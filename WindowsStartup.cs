using System;
using Microsoft.Win32;

namespace TrayDesk
{
    /// <summary>
    /// Toggles the per-user "run at logon" entry under
    /// HKCU\Software\Microsoft\Windows\CurrentVersion\Run for the current executable.
    /// </summary>
    public static class WindowsStartup
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "TrayDesk";

        // Quoted so a path with spaces is launched correctly.
        private static string Command => $"\"{Environment.ProcessPath}\"";

        /// <summary>True only when the entry exists and points at *this* executable.</summary>
        public static bool IsEnabled
        {
            get
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey);
                return key?.GetValue(ValueName) as string == Command;
            }
        }

        public static void Set(bool enabled)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            if (key is null)
            {
                return;
            }

            if (enabled)
            {
                // Always (re)write the current path so a stale entry self-heals.
                key.SetValue(ValueName, Command);
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
        }
    }
}
