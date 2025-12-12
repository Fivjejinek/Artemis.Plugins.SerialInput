using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared;
namespace Artemis.Plugins.SerialInput.ViewModels
{
    public class SerialInputConfigurationViewModel : ModuleViewModel
    {
        public PluginSetting<string> ComPort { get; }
        public PluginSetting<int> BaudRate { get; }

        public SerialInputConfigurationViewModel(ArduinoPinsModule module) : base(module)
        {
            ComPort = module.ComPortSetting;
            BaudRate = module.BaudRateSetting;
        }
    }
}
