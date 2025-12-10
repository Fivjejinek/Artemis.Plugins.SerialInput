using Artemis.Core;

namespace ArduinoPinsPlugin
{
    public class ArduinoPinsSettings : PluginConfiguration
    {
        [PluginSetting]
        public string ComPort { get; set; } = "COM3";

        [PluginSetting]
        public int BaudRate { get; set; } = 9600;
    }
}
