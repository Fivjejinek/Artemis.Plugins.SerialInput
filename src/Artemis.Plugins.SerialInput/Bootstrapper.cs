using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.Plugins.SerialInput.ViewModels;

namespace Artemis.Plugins.SerialInput;

public class Bootstrapper : PluginBootstrapper
{
    public override void OnPluginLoaded(Plugin plugin)
    {
        // Register the configuration dialog with the ViewModel type
        plugin.ConfigurationDialog = new PluginConfigurationDialog<SerialInputConfigurationViewModel>();
    }

    public override void OnPluginEnabled(Plugin plugin)
    {
        plugin.Register<ArduinoPinsModule>();
    }

    public override void OnPluginDisabled(Plugin plugin) { }
}
