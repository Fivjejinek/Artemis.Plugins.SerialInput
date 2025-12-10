using System;
using System.Collections.Generic;
using System.IO.Ports;
using Artemis.Core;
using Artemis.Core.Modules;
using Serilog;

namespace Artemis.Plugins.SerialInput;

public class ArduinoPinsModule : Module<ArduinoPinsDataModel>
{
    private readonly PluginSetting<string> _boardSetting;
    private readonly PluginSetting<string> _comPortSetting;
    private readonly PluginSetting<int> _baudRateSetting;
    private readonly ILogger _logger;
    private SerialPort? _serial;

    public ArduinoPinsModule(PluginSettings pluginSettings, ILogger logger)
    {
        _logger = logger;
        _boardSetting = pluginSettings.GetSetting("Board", "Uno");
        _comPortSetting = pluginSettings.GetSetting("ComPort", "COM3");
        _baudRateSetting = pluginSettings.GetSetting("BaudRate", 9600);

        _boardSetting.PropertyChanged += (_, __) => RestartSerial();
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

            var prop = GetType().GetProperty($"Pin{pin}");
            if (prop != null) prop.SetValue(DataModel, state);
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

            var prop = GetType().GetProperty($"A{index}");
            if (
