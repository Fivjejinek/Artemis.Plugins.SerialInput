using System;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.SerialInput
{
    public class Bootstrapper : PluginBootstrapper
    {
        // We keep these for local use but we read values at runtime via PluginCompat
        private const string ComPortKey = "ComPort";
        private const string BaudRateKey = "BaudRate";

        public override void OnPluginLoaded(Plugin plugin)
        {
            // No compile-time dependency on CreateSetting/GetSetting; we just ensure defaults exist.
            // Optionally you can attempt to create settings via reflection if needed.
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
            // Register the module using reflection helper
            PluginCompat.RegisterModule(plugin, typeof(ArduinoPinsModule));
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}
