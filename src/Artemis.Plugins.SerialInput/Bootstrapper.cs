using Artemis.Core;
using Artemis.UI.Shared;

namespace ArduinoPinsPlugin
{
    public class Bootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
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
