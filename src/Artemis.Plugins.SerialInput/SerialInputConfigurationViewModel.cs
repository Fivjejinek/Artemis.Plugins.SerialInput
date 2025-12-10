using Artemis.Core;
using ReactiveUI;

namespace Artemis.Plugins.SerialInput.ViewModels;

public class SerialInputConfigurationViewModel : ReactiveObject
{
    public PluginSetting<string> ComPort { get; }
    public PluginSetting<int> BaudRate { get; }

    public SerialInputConfigurationViewModel(PluginSettings settings)
    {
        ComPort = settings.GetSetting("ComPort", "COM3");
        BaudRate = settings.GetSetting("BaudRate", 9600);
    }
}
