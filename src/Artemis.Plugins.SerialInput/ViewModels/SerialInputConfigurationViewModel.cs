using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared;
namespace Artemis.Plugins.SerialInput.ViewModels
{
    public class SerialInputConfigurationViewModel : PluginConfigurationViewModel
    {
        public PluginSetting<string> ComPort { get; }
        public PluginSetting<int> BaudRate { get; }

        public SerialInputConfigurationViewModel(Plugin plugin, PluginSettings settings)
            : base(plugin)
        {
            ComPort = settings.GetSetting("ComPort", string.Empty);
            BaudRate = settings.GetSetting("BaudRate", 0);
        }
    }
}
