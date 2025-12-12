using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using Artemis.Core;
using Artemis.Core.Modules;
using Serilog;

namespace Artemis.Plugins.SerialInput
{
    public class ArduinoPinsModule : Module<ArduinoPinsDataModel>
    {
        private readonly PluginSetting<string> _comPortSetting;
        private readonly PluginSetting<int> _baudRateSetting;
        private readonly ILogger _logger;

        private SerialPort? _serial;
        private bool _handshakeDone = false;
        private string? _boardType;

        private DateTimeOffset _lastMessageTime = DateTimeOffset.MinValue;
        private DateTimeOffset _lastHandshakeAttempt = DateTimeOffset.MinValue;

        private readonly StringBuilder _rxBuffer = new StringBuilder(1024);

        private readonly Dictionary<int, PropertyInfo> _digitalProps = new();
        private readonly Dictionary<int, PropertyInfo> _analogProps = new();

        public ArduinoPinsModule(PluginSettings pluginSettings, ILogger logger)
        {
            _logger = logger;

            _comPortSetting = pluginSettings.GetSetting("ComPort", string.Empty);
            _baudRateSetting = pluginSettings.GetSetting("BaudRate", 0);

            _comPortSetting.PropertyChanged += (_, __) => RestartSerial();
            _baudRateSetting.PropertyChanged += (_, __) => RestartSerial();

            CacheProperties();
        }

        public override List<IModuleActivationRequirement> ActivationRequirements => new();

        public override void Enable() => OpenSerial();

        public override void Disable() => CloseSerialIfOpen();

        public override void Update(double deltaTime)
        {
            if (!AreSettingsValid())
            {
                DataModel.IsConnected = false;
                return;
            }

            if (_serial == null || !_serial.IsOpen)
            {
                DataModel.IsConnected = false;
                return;
            }

            try
            {
                // Handshake phase: retry every 1 second until success
                if (!_handshakeDone)
                {
                    if ((DateTimeOffset.UtcNow - _lastHandshakeAttempt).TotalSeconds >= 1)
                    {
                        _serial.Write(new byte[] { 0x01 }, 0, 1);
                        _lastHandshakeAttempt = DateTimeOffset.UtcNow;
                    }

                    ReadIntoBuffer();
                    if (TryConsumeLine(out string line))
                    {
                        string response = line.Trim();
                        if (response == "Uno" || response == "Mega")
                        {
                            _boardType = response;
                            _handshakeDone = true;
                            DataModel.BoardType = _boardType;
                            DataModel.IsConnected = true;
                            _lastMessageTime = DateTimeOffset.UtcNow;
                        }
                    }
                    return;
                }

                // Normal frame processing (no 0x02 acknowledgment)
                ReadIntoBuffer();
                while (TryConsumeLine(out string frame))
                {
                    _lastMessageTime = DateTimeOffset.UtcNow;

                    if (frame.Length == 1 && frame[0] == (char)0x03)
                    {
                        // Heartbeat
                        DataModel.IsConnected = true;
                    }
                    else
                    {
                        ParseFrame(frame);
                        DataModel.IsConnected = true;
                        DataModel.LastUpdated = DateTimeOffset.UtcNow;
                    }
                }

                // Timeout if no message for 15s
                if ((DateTimeOffset.UtcNow - _lastMessageTime).TotalSeconds > 15)
                    DataModel.IsConnected = false;
            }
            catch (Exception e)
            {
                _logger?.Error(e, "Serial processing error");
            }
        }

        private bool AreSettingsValid()
        {
            if (string.IsNullOrWhiteSpace(_comPortSetting.Value))
                return false;
            if (_baudRateSetting.Value <= 0)
                return false;
            return true;
        }

        private void CacheProperties()
        {
            var type = typeof(ArduinoPinsDataModel);

            // Digital pins Pin2..Pin53
            for (int p = 2; p <= 53; p++)
            {
                var prop = type.GetProperty($"Pin{p}");
                if (prop != null)
                    _digitalProps[p] = prop;
            }

            // Analog pins A0..A15
            for (int a = 0; a <= 15; a++)
            {
                var prop = type.GetProperty($"A{a}");
                if (prop != null)
                    _analogProps[a] = prop;
            }
        }

        private void ParseFrame(string line)
        {
            var blocks = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var block in blocks)
            {
                if (block.StartsWith("D:"))
                    ParseDigital(block.AsSpan(2));
                else if (block.StartsWith("A:"))
                    ParseAnalog(block.AsSpan(2));
            }
        }

        private void ParseDigital(ReadOnlySpan<char> csv)
        {
            int start = 0;
            while (start < csv.Length)
            {
                int comma = csv.Slice(start).IndexOf(',');
                ReadOnlySpan<char> pair = comma >= 0 ? csv.Slice(start, comma) : csv.Slice(start);

                int eq = pair.IndexOf('=');
                if (eq > 0)
                {
                    var pinSpan = pair.Slice(0, eq);
                    var valSpan = pair.Slice(eq + 1);

                    if (int.TryParse(pinSpan, out int pin))
                    {
                        if (pin != 0 && pin != 1 && _digitalProps.TryGetValue(pin, out var prop))
                        {
                            bool state = valSpan.Length == 1 && valSpan[0] == '1';
                            prop.SetValue(DataModel, state);
                        }
                    }
                }

                if (comma < 0) break;
                start += comma + 1;
            }
        }

        private void ParseAnalog(ReadOnlySpan<char> csv)
        {
            int start = 0;
            while (start < csv.Length)
            {
                int comma = csv.Slice(start).IndexOf(',');
                ReadOnlySpan<char> pair = comma >= 0 ? csv.Slice(start, comma) : csv.Slice(start);

                int eq = pair.IndexOf('=');
                if (eq > 0)
                {
                    var idxSpan = pair.Slice(0, eq);
                    var valSpan = pair.Slice(eq + 1);

                    if (int.TryParse(idxSpan, out int index) && int.TryParse(valSpan, out int val))
                    {
                        val = Math.Clamp(val, 0, 1023);
                        if (_analogProps.TryGetValue(index, out var prop))
                            prop.SetValue(DataModel, val);
                    }
                }

                if (comma < 0) break;
                start += comma + 1;
            }
        }

        private void ReadIntoBuffer()
        {
            if (_serial == null) return;

            try
            {
                int toRead = _serial.BytesToRead;
                if (toRead <= 0) return;

                char[] tmp = new char[toRead];
                int read = _serial.Read(tmp, 0, toRead);
                if (read > 0)
                    _rxBuffer.Append(tmp, 0, read);
            }
            catch (TimeoutException)
            {
                // ignore
            }
        }

        private bool TryConsumeLine(out string line)
        {
            line = string.Empty;
            for (int i = 0; i < _rxBuffer.Length; i++)
            {
                if (_rxBuffer[i] == '\n')
                {
                    line = _rxBuffer.ToString(0, i).TrimEnd('\r');
                    _rxBuffer.Remove(0, i + 1);
                    return true;
                }
            }

            // Special case: heartbeat 0x03 (single char, no newline)
            if (_rxBuffer.Length == 1 && _rxBuffer[0] == (char)0x03)
            {
                line = _rxBuffer.ToString();
                _rxBuffer.Clear();
                return true;
            }

            return false;
        }

        private void OpenSerial()
        {
            try
            {
                CloseSerialIfOpen();

                if (!AreSettingsValid())
                {
                    _logger?.Warning("Serial settings invalid. Please set COM port and baud rate.");
                    return;
                }

                _serial = new SerialPort(_comPortSetting.Value!, _baudRateSetting.Value)
                {
                    ReadTimeout = 200,
                    NewLine = "\n"
                };
                _serial.Open();

                _handshakeDone = false;
                _boardType = null;
                _rxBuffer.Clear();
                _lastHandshakeAttempt = DateTimeOffset.MinValue;

                _logger?.Information($"Serial opened on {_comPortSetting.Value} at {_baudRateSetting.Value} baud.");
            }
            catch (Exception e)
            {
                _logger?.Error(e, "Failed to open serial port");
                _serial = null;
            }
        }

        private void CloseSerialIfOpen()
        {
            try
            {
                if (_serial?.IsOpen == true)
                    _serial.Close();
            }
            catch (Exception e)
            {
                _logger?.Error(e, "Failed to close serial port");
            }
            finally
            {
                _serial = null;
                _handshakeDone = false;
                _boardType = null;
                _rxBuffer.Clear();

                DataModel.IsConnected = false;
                DataModel.BoardType = "";
            }
        }

        private void RestartSerial()
        {
            CloseSerialIfOpen();
            OpenSerial();
        }
    }
}
