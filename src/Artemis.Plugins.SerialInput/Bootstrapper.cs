using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.SerialInput
{
    public class Bootstrapper : PluginBootstrapper
    {
        private PluginSetting<string> _comPort;
        private PluginSetting<int> _baudRate;

        public override void OnPluginLoaded(Plugin plugin)
        {
            _comPort = plugin.GetSetting("ComPort", "COM3");
            _baudRate = plugin.GetSetting("BaudRate", 9600);
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            plugin.RegisterModule(new ArduinoPinsModule(_comPort.Value, _baudRate.Value));
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
