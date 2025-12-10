using Artemis.Core.Modules;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace Artemis.Plugins.SerialInput
{
    public class ArduinoPinsModule : Module<ArduinoPinsDataModel>
    {
        private SerialPort? _serial;

        public override void Enable()
        {
            // Read settings via reflection helper with safe defaults
            string comPort = PluginCompat.GetSettingValue(Plugin, "ComPort", "COM3");
            int baudRate = PluginCompat.GetSettingValue(Plugin, "BaudRate", 9600);

            try
            {
                _serial = new SerialPort(comPort, baudRate);
                _serial.Open();
            }
            catch (Exception ex)
            {
                // Log or swallow depending on your logging setup; avoid crashing the module
                // Example: Console.WriteLine($"Serial open failed: {ex}");
            }
        }

        public override void Disable()
        {
            try
            {
                _serial?.Close();
            }
            catch { }
            finally
            {
                _serial = null;
            }
        }

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
            catch
            {
                // ignore malformed input
            }
        }

        public override List<IModuleActivationRequirement> ActivationRequirements => new();
    }
}
