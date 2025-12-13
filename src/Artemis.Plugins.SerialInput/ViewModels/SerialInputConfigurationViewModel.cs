using System.Reactive;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.Plugins.SerialInput.ViewModels
{
    public class SerialInputConfigurationViewModel : PluginConfigurationViewModel
    {
        private readonly PluginSetting<string> _comPort;
        private readonly PluginSetting<int> _baudRate;

        public SerialInputConfigurationViewModel(Plugin plugin, PluginSettings settings)
            : base(plugin)
        {
            _comPort = settings.GetSetting("ComPort", string.Empty);
            _baudRate = settings.GetSetting("BaudRate", 250000);

            ComPort = _comPort.Value;
            BaudRate = _baudRate.Value;

            SaveChanges = ReactiveCommand.Create(ExecuteSaveChanges);
            Cancel = ReactiveCommand.Create(ExecuteCancel);
        }

        public string ComPort { get; set; }
        public int BaudRate { get; set; }

        public ReactiveCommand<Unit, Unit> SaveChanges { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        private void ExecuteSaveChanges()
        {
            _comPort.Value = ComPort;
            _baudRate.Value = BaudRate;

            _comPort.Save();
            _baudRate.Save();

            Close();
        }

        private void ExecuteCancel()
        {
            _comPort.RejectChanges();
            _baudRate.RejectChanges();

            Close();
        }
    }
}
