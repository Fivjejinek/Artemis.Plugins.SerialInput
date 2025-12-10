using Artemis.Plugins.SerialInput.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.Plugins.SerialInput.Views
{
    public partial class SerialInputConfigurationView : ReactiveUserControl<SerialInputConfigurationViewModel>
    {
        public SerialInputConfigurationView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
