using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;

namespace DeviceDriver
{
    // The driver communicates with a device via RS-232 serial communication. 
    public class DeviceDriver
    {
        private SerialPort _serialPort;

        public DeviceDriver(
            string portName,
            int baudRate = 9600,
            Parity? parity = null,
            int? dataBits = null,
            StopBits? stopBits = null,
            Handshake? handshake = null,
            int readTimeout = 500,
            int writeTimeout = 500)
        {
            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Configure driver communication settings: Baud Rate, Stop Bits, Data Bits, Parity, etc.
            foreach (string availablePort in SerialPort.GetPortNames())
            {
                // If the provided portName is not valid, use the default portName
                if (portName == availablePort)
                {
                    _serialPort.PortName = portName;
                }
                else
                {
                    _serialPort.PortName = _serialPort.PortName;
                }
            }
            _serialPort.BaudRate = baudRate;
            _serialPort.Parity = parity == null ? _serialPort.Parity : (Parity)parity;
            _serialPort.DataBits = dataBits == null ? _serialPort.DataBits : (int)dataBits;
            _serialPort.StopBits = stopBits == null ? _serialPort.StopBits : (StopBits)stopBits;
            _serialPort.Handshake = handshake == null ? _serialPort.Handshake : (Handshake)handshake;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = readTimeout;
            _serialPort.WriteTimeout = writeTimeout;
        }

        /// <summary>
        /// Send command to the device
        /// </summary>
        /// <param name="message">String command without carriage return (\r) character to send to the device</param>
        /// <returns>Device response without carriage return (\r) and newline (\n) characters</returns>
        private string SendCommand(string message)
        {
            message += "\r";

            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
            _serialPort.Write(message);

            // Response is trimmed of \r\n when read
            // TODO: this response parsing assumes that responses are complete; to make more robust
            string response = _serialPort.ReadTo("\r\n") + _serialPort.ReadTo("\r\n");
            if (response.StartsWith("ok", StringComparison.OrdinalIgnoreCase))
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }

                // Remove "ok" from the response string that is returned, and trim spaces
                return response.Substring("ok".Length).Trim();
            }
            else if (response.StartsWith("err"))
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                switch (response)
                {
                    case "err \"Invalid Value\"":
                        throw new ArgumentException("Invalid value");
                    case "err \"Missing Parameter\"":
                        throw new ArgumentException("Missing Parameter");
                    case "err \"Invalid Parameter\"":
                        throw new ArgumentException("Invalid Parameter");
                    case "err":
                    default:
                        throw new Exception("A generic error occured");
                }
            }
            else
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                throw new IOException("Device response was not understood.");
            }
        }

        /// <summary>
        /// Turn on the device
        /// </summary>
        public void TurnOn()
        {
            string response = SendCommand("setpower on 1");
            Debug.WriteLine(response);
        }

        /// <summary>
        /// Turn off the device
        /// </summary>
        public void TurnOff()
        {
            string response = SendCommand("setpower on 0");
            Debug.WriteLine(response);
        }

        /// <summary>
        /// Get the Power State (turned on or off)
        /// </summary>
        /// <returns>True if device is on False if it is off</returns>
        public bool IsOn()
        {
            string[] response = SendCommand("getpowerstatus").Split(' ');
            Int16 isOn = Int16.Parse(response[1]);
            if (isOn == 1)
            {
                return true;
            }
            else if (isOn == 0)
            {
                return false;
            }
            else
            {
                // If the response is neither 0 nor 1, then either the device has a different API,
                // or there's a physical error or physical damage, hence the IOException
                throw new IOException("Turn on/off response code was neither 0 nor 1.");
            }
        }

        /// <summary>
        /// Get the Serial Number of the device
        /// </summary>
        /// <returns>String of the Serial Number</returns>
        public string GetSerialNumber()
        {
            string[] response = SendCommand("getserialnumber").Split(' ');
            string serialNumber = response[1];
            Debug.WriteLine(serialNumber);

            return serialNumber;
        }
    }
}
