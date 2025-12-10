using System;
using System.Collections.Generic;
using System.IO.Ports;
using Artemis.Core;
using Artemis.Core.Modules;
using Serilog;

namespace Artemis.Plugins.SerialInput;

public class ArduinoPinsModule : Module<ArduinoPinsDataModel>
{
    private readonly PluginSetting<string> _comPortSetting;
    private readonly PluginSetting<int> _baudRateSetting;
    private readonly ILogger _logger;
    private SerialPort? _serial;

    public ArduinoPinsModule(PluginSettings pluginSettings, ILogger logger)
    {
        _logger = logger;
        _comPortSetting = pluginSettings.GetSetting("ComPort", "COM3");
        _baudRateSetting = pluginSettings.GetSetting("BaudRate", 9600);

        _comPortSetting.PropertyChanged += (_, __) => RestartSerial();
        _baudRateSetting.PropertyChanged += (_, __) => RestartSerial();
    }

    public override List<IModuleActivationRequirement> ActivationRequirements => new();

    public override void Enable() => OpenSerial();

    public override void Disable() => CloseSerialIfOpen();

    public override void Update(double deltaTime)
    {
        if (_serial == null || !_serial.IsOpen) return;

        try
        {
            if (_serial.BytesToRead == 0) return;
            string line = _serial.ReadLine();
            var parts = line.Split(',');

            foreach (var part in parts)
            {
                var kv = part.Split(':');
                if (kv.Length != 2) continue;
                if (!int.TryParse(kv[0], out int pin)) continue;
                bool state = kv[1] == "1";

                switch (pin)
                {
                    case 2: DataModel.Pin2 = state; break;
                    case 3: DataModel.Pin3 = state; break;
                    case 4: DataModel.Pin4 = state; break;
                    case 5: DataModel.Pin5 = state; break;
                    case 6: DataModel.Pin6 = state; break;
                    case 7: DataModel.Pin7 = state; break;
                    case 8: DataModel.Pin8 = state; break;
                    case 9: DataModel.Pin9 = state; break;
                    case 10: DataModel.Pin10 = state; break;
                    case 11: DataModel.Pin11 = state; break;
                    case 12: DataModel.Pin12 = state; break;
                    case 13: DataModel.Pin13 = state; break;
                }
            }
        }
        catch (Exception e)
        {
            _logger?.Error(e.ToString());
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
        }
        catch (Exception e)
        {
            _logger?.Error($"Failed to open serial port: {e}");
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
            _logger?.Error(e.ToString());
        }
        finally
        {
            _serial = null;
        }
    }

    private void RestartSerial()
    {
        CloseSerialIfOpen();
        OpenSerial();
    }
}
