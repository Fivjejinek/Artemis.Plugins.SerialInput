using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.SerialInput
{
    public class Bootstrapper : PluginBootstrapper
    {
        private PluginSetting<string> _comPort = null!;
        private PluginSetting<int> _baudRate = null!;

        public override void OnPluginLoaded(Plugin plugin)
        {
            // Create settings using PluginBootstrapper API (latest Artemis)
            _comPort = CreateSetting("ComPort", "COM3");
            _baudRate = CreateSetting("BaudRate", 9600);
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            // Register module directly with the plugin (latest API)
            plugin.RegisterModule<ArduinoPinsModule>();
        }

        public override void OnPluginDisabled(Plugin plugin) { }
    }
}
