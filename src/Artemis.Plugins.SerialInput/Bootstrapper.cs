using Artemis.Core;

namespace Artemis.Plugins.SerialInput
{
    public class Bootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            // Nothing required here for settings; the module will create/read them via PluginSettings.
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            // Register the module with the plugin host.
            plugin.Register<ArduinoPinsModule>();
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
