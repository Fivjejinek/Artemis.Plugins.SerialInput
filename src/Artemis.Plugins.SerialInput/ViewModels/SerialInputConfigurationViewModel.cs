using System.Reactive;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.Plugins.SerialInput.ViewModels
{
    public class SerialInputConfigurationViewModel : PluginConfigurationViewModel
    {
        private readonly PluginSetting<string> _comPortSetting;
        private readonly PluginSetting<int> _baudRateSetting;

        private string _comPort;
        private int _baudRate;

        public SerialInputConfigurationViewModel(Plugin plugin, PluginSettings settings)
            : base(plugin)
        {
            _comPortSetting = settings.GetSetting("ComPort", string.Empty);
            _baudRateSetting = settings.GetSetting("BaudRate", 115200);

            _comPort = _comPortSetting.Value;
            _baudRate = _baudRateSetting.Value;

            SaveChanges = ReactiveCommand.Create(ExecuteSaveChanges);
            Cancel = ReactiveCommand.Create(ExecuteCancel);
        }

        public string ComPort
        {
            get => _comPort;
            set => this.RaiseAndSetIfChanged(ref _comPort, value);
        }

        public int BaudRate
        {
            get => _baudRate;
            set => this.RaiseAndSetIfChanged(ref _baudRate, value);
        }

        public ReactiveCommand<Unit, Unit> SaveChanges { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        private void ExecuteSaveChanges()
        {
            _comPortSetting.Value = ComPort;
            _baudRateSetting.Value = BaudRate;

            _comPortSetting.Save();
            _baudRateSetting.Save();

            Close();
        }

        private void ExecuteCancel()
        {
            _comPortSetting.RejectChanges();
            _baudRateSetting.RejectChanges();

            Close();
        }
    }
}
