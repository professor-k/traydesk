using System;
using System.IO.Ports;

namespace TrayDesk
{
    public class SerialPortParser : IDisposable
    {
        private readonly SerialPort _port;
        private string _buffer = string.Empty;

        public event EventHandler<int> DataReceived;

        public SerialPortParser(string portName)
        {
            _port = new(portName, 9600, Parity.None, 8, StopBits.One);
            _port.DataReceived += port_DataReceived;
            _port.Open();
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Sometimes data received over COM is not complete, e.g. it can be broken between two digits.
            // So we're prepending whatever left from last incomplete message
            string value = _buffer + _port.ReadExisting();
            var separator = new[] { '\r', '\n' };
            var lastTerminator = value.LastIndexOfAny(separator);
            _buffer = value.Substring(lastTerminator);

            var parts = value.Substring(0, lastTerminator).Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                DataReceived?.Invoke(this, int.Parse(part));
            }
        }

        public void Dispose()
        {
            _port?.Dispose();
        }
    }
}