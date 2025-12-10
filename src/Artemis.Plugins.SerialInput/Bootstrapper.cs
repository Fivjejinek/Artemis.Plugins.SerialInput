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
            // Create settings directly from the plugin
            _comPort = plugin.Settings.GetSetting("ComPort", "COM3");
            _baudRate = plugin.Settings.GetSetting("BaudRate", 9600);
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            // Register your module with the plugin
            plugin.Modules.RegisterModule(() => new ArduinoPinsModule(_comPort.Value, _baudRate.Value));
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
