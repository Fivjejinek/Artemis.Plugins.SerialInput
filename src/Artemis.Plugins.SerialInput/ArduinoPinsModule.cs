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
        private int _missedResponses = 0;

        private DateTimeOffset _lastMessageTime = DateTimeOffset.MinValue;

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
                // Handshake phase
                if (!_handshakeDone)
                {
                    _serial.Write(new byte[] { 0x01 }, 0, 1);
                    ReadIntoBuffer();
                    if (TryConsumeLine(out string line))
                    {
                        string response = line.Trim();
                        if (response == "Uno" || response == "Mega")
                        {
                            _boardType = response;
                            _handshakeDone = true;
                            _missedResponses = 0;
                            DataModel.BoardType = _boardType ?? "";
                            DataModel.IsConnected = true;
                            _lastMessageTime = DateTimeOffset.UtcNow;
                        }
                    }
                    return;
                }

                // Read incoming frames
                ReadIntoBuffer();

                while (TryConsumeLine(out string frame))
                {
                    _lastMessageTime = DateTimeOffset.UtcNow;

                    // Heartbeat check: raw 0x03
                    if (frame.Length == 1 && frame[0] == (char)0x03)
                    {
                        DataModel.IsConnected = true;
                    }
                    else
                    {
                        ParseFrame(frame);
                        DataModel.IsConnected = true;
                        DataModel.LastUpdated = DateTimeOffset.UtcNow;

                        // Send acknowledgment 0x02
                        _serial.Write(new byte[] { 0x02 }, 0, 1);
                    }
                }

                // Timeout: if no message for 15s, mark disconnected
                if ((DateTimeOffset.UtcNow - _lastMessageTime).TotalSeconds > 15)
                {
                    DataModel.IsConnected = false;
                }
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



            for (int p = 2; p <= 53; p
