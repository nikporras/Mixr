using Mixr.UI.ViewModels;
using Mixr.Actions.DataModel.Serialization;

namespace Mixr.Actions.ViewModel
{
    class ForegroundAppViewModel : SettingsAppItemViewModel
    {
        public ForegroundAppViewModel()
        {
            Id = AppRef.ForegroundAppId;
            DisplayName = Properties.Resources.ForegroundAppText;
        }
    }
}
