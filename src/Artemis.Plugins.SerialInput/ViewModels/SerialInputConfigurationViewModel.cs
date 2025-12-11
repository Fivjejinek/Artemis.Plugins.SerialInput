using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.SerialInput.ViewModels
{
    public class SerialInputConfigurationViewModel : PluginConfigurationViewModel
    {
        public PluginSetting<string> ComPort { get; }
        public PluginSetting<int> BaudRate { get; }
        public PluginSetting<double> UpdateRate { get; }

        public SerialInputConfigurationViewModel(Plugin plugin, PluginSettings settings)
            : base(plugin)
        {
            ComPort = settings.GetSetting("ComPort", "COM11");       // default COM11
            BaudRate = settings.GetSetting("BaudRate", 115200);      // default 115200
            UpdateRate = settings.GetSetting("UpdateRate", 0.1);     // default 0.1s
        }
    }
}
