using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
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
        private double _elapsedSinceLastRequest = 0;

        public ArduinoPinsModule(PluginSettings pluginSettings, ILogger logger)
        {
            _logger = logger;
            _comPortSetting = pluginSettings.GetSetting("ComPort", "COM3");
            _baudRateSetting = pluginSettings.GetSetting("BaudRate", 9600);

            _comPortSetting.PropertyChanged += (_, __) => RestartSerial();
            _baudRateSetting.PropertyChanged += (_, __) => RestartSerial();
        }

        public override List<IModuleActivationRequirement> ActivationRequirements => new();

        public override void Enable()
        {
            OpenSerial();
            if (_serial != null && _serial.IsOpen)
            {
                // Ask "who are you"
                _serial.Write(new byte[] { 0x01 }, 0, 1);
            }
        }

        public override void Disable() => CloseSerialIfOpen();

        public override void Update(double deltaTime)
        {
            if (_serial == null || !_serial.IsOpen) return;

            try
            {
                if (!_handshakeDone && _serial.BytesToRead > 0)
                {
                    string response = _serial.ReadLine().Trim();
                    if (response == "Uno" || response == "Mega")
                    {
                        _boardType = response;
                        _handshakeDone = true;
                        _elapsedSinceLastRequest = 0;
                    }
                    return;
                }

                if (_handshakeDone)
                {
                    _elapsedSinceLastRequest += deltaTime;

                    // Only send request once per second
                    if (_elapsedSinceLastRequest >= 1.0)
                    {
                        _serial.Write(new byte[] { 0x02 }, 0, 1);
                        _elapsedSinceLastRequest = 0;
                    }

                    while (_serial.BytesToRead > 0)
                    {
                        string line = _serial.ReadLine().Trim();
                        var blocks = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var block in blocks)
                        {
                            if (block.StartsWith("D:")) ParseDigital(block.Substring(2));
                            else if (block.StartsWith("A:")) ParseAnalog(block.Substring(2));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e, "Serial read error");
            }
        }

        private void ParseDigital(string csv)
        {
            foreach (var p in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = p.Split('=');
                if (kv.Length != 2) continue;
                if (!int.TryParse(kv[0], out int pin)) continue;
                bool state = kv[1] == "1";

                if (pin == 0 || pin == 1) continue;

                PropertyInfo? prop = typeof(ArduinoPinsDataModel).GetProperty($"Pin{pin}");
                if (prop != null)
                    prop.SetValue(DataModel, state);
            }
        }

        private void ParseAnalog(string csv)
        {
            foreach (var p in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = p.Split('=');
                if (kv.Length != 2) continue;
                if (!int.TryParse(kv[0], out int index)) continue;
                if (!int.TryParse(kv[1], out int val)) continue;
                val = Math.Clamp(val, 0, 1023);

                PropertyInfo? prop = typeof(ArduinoPinsDataModel).GetProperty($"A{index}");
                if (prop != null)
                    prop.SetValue(DataModel, val);
            }
        }

        private void OpenSerial()
        {
            try
            {
                CloseSerialIfOpen();
                _serial = new SerialPort(_comPortSetting.Value ?? "COM3", _baudRateSetting.Value)
                {
                    ReadTimeout = 500,
                    NewLine = "\n"
                };
                _serial.Open();
                _logger?.Information($"Opened serial port {_comPortSetting.Value} at {_baudRateSetting.Value} baud");
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
            }
        }

        private void RestartSerial()
        {
            CloseSerialIfOpen();
            OpenSerial();
        }
    }
}
