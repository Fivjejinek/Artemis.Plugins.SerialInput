using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.Plugins.SerialInput.Views;
using Artemis.Plugins.SerialInput.ViewModels;

namespace Artemis.Plugins.SerialInput;

public class Bootstrapper : PluginBootstrapper
{
    public override void OnPluginLoaded(Plugin plugin)
    {
        // Only pass the View type here
        plugin.ConfigurationDialog = new PluginConfigurationDialog<SerialInputConfigurationView>();
    }

    public override void OnPluginEnabled(Plugin plugin)
    {
        plugin.Register<ArduinoPinsModule>();
    }

    public override void OnPluginDisabled(Plugin plugin) { }
}
