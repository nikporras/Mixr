

namespace Mixr.UI.ViewModels
{
    public class MixrLegacySettingsPageViewModel : SettingsPageViewModel
    {
        public bool UseLegacyIcon
        {
            get => _settings.UseLegacyIcon;
            set => _settings.UseLegacyIcon = value;
        }

        private readonly AppSettings _settings;

        public MixrLegacySettingsPageViewModel(AppSettings settings) : base(null)
        {
            _settings = settings;
            Title = Properties.Resources.LegacySettingsPageText;
            Glyph = "\xE825";
        }
    }
}