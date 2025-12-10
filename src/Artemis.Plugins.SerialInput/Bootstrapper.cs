using System;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.SerialInput
{
    public class Bootstrapper : PluginBootstrapper
    {
        private const string ComPortKey = "ComPort";
        private const string BaudRateKey = "BaudRate";

        public override void OnPluginLoaded(Plugin plugin)
        {
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            // Register the module using your compatibility helper
            PluginCompat.RegisterModule(plugin, typeof(ArduinoPinsModule));
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
