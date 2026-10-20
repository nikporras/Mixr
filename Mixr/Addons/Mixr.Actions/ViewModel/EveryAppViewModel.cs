using Mixr.UI.ViewModels;
using Mixr.Actions.DataModel.Serialization;

namespace Mixr.Actions.ViewModel
{
    class EveryAppViewModel : SettingsAppItemViewModel
    {
        public EveryAppViewModel()
        {
            DisplayName = Properties.Resources.EveryAppText;
            Id = AppRef.EveryAppId;
        }
    }
}
