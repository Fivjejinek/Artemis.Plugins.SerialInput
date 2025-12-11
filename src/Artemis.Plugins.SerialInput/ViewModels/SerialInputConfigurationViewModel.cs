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
            // No defaults: start empty and require user input
            ComPort = settings.GetSetting("ComPort", string.Empty);
            BaudRate = settings.GetSetting("BaudRate", 0);
            UpdateRate = settings.GetSetting("UpdateRate", 0.0);
        }
    }
}
