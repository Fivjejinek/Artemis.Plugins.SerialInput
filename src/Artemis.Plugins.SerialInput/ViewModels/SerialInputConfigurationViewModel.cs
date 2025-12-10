using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.SerialInput.ViewModels
{
    public class SerialInputConfigurationViewModel : PluginConfigurationViewModel
    {
        public PluginSetting<string> Board { get; }
        public PluginSetting<string> ComPort { get; }
        public PluginSetting<int> BaudRate { get; }

        public string[] BoardOptions { get; } = new[] { "Uno", "Mega" };

        public SerialInputConfigurationViewModel(Plugin plugin, PluginSettings settings)
            : base(plugin)
        {
            Board = settings.GetSetting("Board", "Uno");
            ComPort = settings.GetSetting("ComPort", "COM3");
            BaudRate = settings.GetSetting("BaudRate", 9600);
        }
    }
}
