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
            // Create settings using the bootstrapper’s CreateSetting<T>
            _comPort = CreateSetting("ComPort", "COM3");
            _baudRate = CreateSetting("BaudRate", 9600);
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            // Register your module with the plugin
            plugin.Register<ArduinoPinsModule>();
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
