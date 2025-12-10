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
            // Create settings directly from the bootstrapper
            _comPort = plugin.CreateSetting("ComPort", "COM3");
            _baudRate = plugin.CreateSetting("BaudRate", 9600);
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            // Register your module with the plugin
            plugin.RegisterModule<ArduinoPinsModule>(() =>
                new ArduinoPinsModule(_comPort?.Value ?? "COM3", _baudRate?.Value ?? 9600));
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
