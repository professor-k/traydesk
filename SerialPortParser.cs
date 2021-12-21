using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using log4net;
using Microsoft.Win32;

namespace TrayDesk
{
    public class SerialPortParser : IDisposable
    {
        private SerialPort _port;
        private string _buffer = string.Empty;

        private readonly ILog _log = LogManager.GetLogger(typeof(Program));

        public event EventHandler<int> DataReceived;

        private readonly string[] _arduinoKeys = {
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\usbser\Enum",
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CH341SER_A64\Enum"
        };

        /// <summary>
        /// See https://arduino.stackexchange.com/questions/30808/how-to-detect-arduino-serial-port-programmatically-on-different-platforms/80887
        /// </summary>
        private IEnumerable<(string name, string port)> EnumerateArduinos()
        {
            foreach (var arduinoKey in _arduinoKeys)
            {
                var countObject  = Registry.GetValue(arduinoKey, "Count", null);
                if (countObject is not int count)
                {
                    continue;
                }

                for (int i = 0; i < count; i++)
                {
                    var enumObject = Registry.GetValue(arduinoKey, i.ToString(), null);
                    if (enumObject is not string enumKey)
                    {
                        continue;
                    }

                    var friendlyName = Registry.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{enumKey}", "FriendlyName", null);
                    var portName = Registry.GetValue($@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\{enumKey}\Device Parameters", "PortName", null);
                    if (portName is string port && friendlyName is string name)
                    {
                        yield return (name, port);
                    }
                }
            }
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Sometimes data received over COM is not complete, e.g. it can be broken between two digits.
            // So we're prepending whatever left from last incomplete message
            string value = _buffer + _port.ReadExisting();
            var separator = new[] { '\r', '\n' };
            var lastTerminator = value.LastIndexOfAny(separator);
            _buffer = value[lastTerminator..];

            var parts = value[..lastTerminator].Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                DataReceived?.Invoke(this, int.Parse(part));
            }
        }

        public void TryReopenIfNeeded()
        {
            if (_port is not { IsOpen: true })
            {
                var port = EnumerateArduinos().FirstOrDefault().port;

                if (string.IsNullOrEmpty(port))
                {
                    // No arduino found
                    return;
                }

                _port = new(port, 9600, Parity.None, 8, StopBits.One);
                _port.DataReceived += port_DataReceived;
                _port.Open();

                _log.Info($"Found arduino at {port}.");
            }
        }

        public bool IsOpen => _port.IsOpen;

        public void Dispose()
        {
            _port?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}