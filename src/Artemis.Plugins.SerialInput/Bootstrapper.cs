using Artemis.Core;
using Artemis.UI.Shared;

namespace ArduinoPinsPlugin
{
    public class Bootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            // Define settings with defaults
            plugin.ConfigurationDialog = new PluginConfigurationDialog<ArduinoPinsSettings>();
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            plugin.RegisterModule<ArduinoPinsModule>();
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
