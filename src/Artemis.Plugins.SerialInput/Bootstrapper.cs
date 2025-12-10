using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.SerialInput
{
    public class Bootstrapper : PluginBootstrapper
    {
        private PluginSetting<string>? _comPort;
        private PluginSetting<int>? _baudRate;

        public override void OnPluginLoaded(Plugin plugin)
        {
            // Create settings using plugin.Configuration
            _comPort = plugin.Configuration.GetSetting("ComPort", "COM3");
            _baudRate = plugin.Configuration.GetSetting("BaudRate", 9600);
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            // Register your module directly
            plugin.RegisterModule<ArduinoPinsModule>();
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
