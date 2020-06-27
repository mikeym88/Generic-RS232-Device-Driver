using System;
using System.IO.Ports;
using System.Text.RegularExpressions;


namespace DeviceSimulation
{
    // This program simulates a serial device
    // Connects to COM6 to simulate the device's end of the RS232 connection.
    public class DeviceSimulation
    {
        static SerialPort _serialPort;

        private static bool _isOn;

        static void Main(string[] args)
        {
            string portName = "";

            if (args.Length >= 1)
            {
                portName = args[0];
            }

            if (portName == "" || !portName.StartsWith("com", StringComparison.OrdinalIgnoreCase))
            {
                _serialPort = new SerialPort("COM6");
            }
            else
            {
                _serialPort = new SerialPort(portName);
            }

            Console.WriteLine("Device RS232 Simulation\r\n" +
                "Port: {0}, Baud Rate: {1}\r\n", _serialPort.PortName, _serialPort.BaudRate);

            _serialPort.Open();

            while (true)
            {
                ReadAndRespond();
            }
        }

        public static void ReadAndRespond()
        {
            try
            {
                string message = _serialPort.ReadTo("\r");
                Console.WriteLine($"Message Received: {message}");

                string response = "";

                // Get Serial Number
                if (message == "getserialnumber")
                {
                    response = "\r\nok serialnumber ABCD1234\r\n";
                }
                // Get Power Status: 1 if powered on, 0 if powered off
                else if (message == "getpowerstatus")
                {
                    if (_isOn)
                    {
                        response = "\r\nok on 1\r\n";
                    }
                    else
                    {
                        response = "\r\nok on 0\r\n";
                    }
                }
                else if (message.StartsWith("setpower", StringComparison.OrdinalIgnoreCase))
                {
                    string pattern = @"setpower on (?<power>[^ |\r|\n]*)?";
                    Regex setMuteRegex = new Regex(pattern);
                    int isOn;
                    bool parsed = int.TryParse(Regex.Match(message, pattern).Groups["power"].ToString(),
                        out isOn);
                    if (setMuteRegex.IsMatch(message))
                    {
                        if (parsed)
                        {
                            if (isOn == 1)
                            {
                                _isOn = true;
                                response = "\r\nok\r\n";
                            }
                            else if (isOn == 0)
                            {
                                _isOn = false;
                                response = "\r\nok\r\n";
                            }
                            else
                            {
                                response = "\r\nerr \"Invalid Value\"\r\n";
                            }
                        }
                        else
                        {
                            response = "\r\nerr \"Invalid Value\"\r\n";
                        }
                    }
                    else if (message == "setmute\r")
                    {
                        response = "\r\nerr \"Missing Parameter\"\r\n";
                    }
                    else
                    {
                        response = "\r\nerr \"Invalid Parameter\"\r\n";
                    }
                }
                else
                {
                    response = "\r\nerr\r\n";
                }

                // Print the response and sent it back through the RS232 serial port
                Console.WriteLine($"Response sent: {response}");
                _serialPort.Write(response);
            }
            catch (TimeoutException) { /* Do Nothing */ }
        }
    }
}
