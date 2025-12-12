using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.Plugins.SerialInput
{
    public class ArduinoPinsModuleViewModel : ModuleViewModel
    {
        public PluginSetting<string> ComPortSetting { get; }
        public PluginSetting<int> BaudRateSetting { get; }

        public ArduinoPinsModuleViewModel(ArduinoPinsModule module) : base(module)
        {
            ComPortSetting = module.ComPortSetting;
            BaudRateSetting = module.BaudRateSetting;
        }
    }
}
