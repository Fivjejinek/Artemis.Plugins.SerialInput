using Artemis.Core.Modules;
using System.Collections.Generic;
using System.IO.Ports;

namespace Artemis.Plugins.SerialInput
{
    public class ArduinoPinsModule : Module<ArduinoPinsDataModel>
    {
        private SerialPort? _serial;

        public override void Enable()
        {
            // Read settings directly from the plugin configuration
            string comPort = Plugin.Configuration.GetSetting("ComPort", "COM3").Value;
            int baudRate = Plugin.Configuration.GetSetting("BaudRate", 9600).Value;

            _serial = new SerialPort(comPort, baudRate);
            _serial.Open();
        }

        public override void Disable()
        {
            _serial?.Close();
            _serial = null;
        }

        public override void Update(double deltaTime)
        {
            if (_serial != null && _serial.IsOpen && _serial.BytesToRead > 0)
            {
                try
                {
                    string line = _serial.ReadLine(); // Example: "2:1,3:0,4:1"
                    var parts = line.Split(',');

                    foreach (var part in parts)
                    {
                        var kv = part.Split(':');
                        if (kv.Length != 2) continue;

                        int pin = int.Parse(kv[0]);
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
                    // swallow errors from malformed serial input
                }
            }
        }

        // Required override for Artemis modules
        public override List<IModuleActivationRequirement> ActivationRequirements => new();
    }
}
