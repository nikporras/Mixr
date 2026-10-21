using System.Collections.ObjectModel;
using System.Linq;

namespace Mixr.UI.ViewModels
{
    // Mixr: lets the user hide playback devices and give them friendly names.
    public class MixrDevicesSettingsPageViewModel : SettingsPageViewModel
    {
        public ObservableCollection<DeviceSettingsItemViewModel> Devices { get; }

        public MixrDevicesSettingsPageViewModel(AppSettings settings, DeviceCollectionViewModel devices) : base(null)
        {
            Title = Properties.Resources.DevicesSettingsPageText;
            Glyph = "\xE7F5";
            Devices = new ObservableCollection<DeviceSettingsItemViewModel>(
                devices.AllDevices
                    .OrderBy(d => d.OriginalDisplayName)
                    .Select(d => new DeviceSettingsItemViewModel(settings, d.Id, d.OriginalDisplayName)));
        }
    }

    public class DeviceSettingsItemViewModel : BindableBase
    {
        public string Id { get; }
        public string OriginalName { get; }

        public bool IsShown
        {
            get => !_settings.IsDeviceHidden(Id);
            set
            {
                _settings.SetDeviceHidden(Id, !value);
                RaisePropertyChanged(nameof(IsShown));
            }
        }

        public bool IsInCycle
        {
            get => _settings.IsDeviceInCycle(Id);
            set
            {
                _settings.SetDeviceInCycle(Id, value);
                RaisePropertyChanged(nameof(IsInCycle));
            }
        }

        public bool IsAutoSwitch
        {
            get => _settings.IsDeviceAutoSwitch(Id);
            set
            {
                _settings.SetDeviceAutoSwitch(Id, value);
                RaisePropertyChanged(nameof(IsAutoSwitch));
            }
        }

        public string CustomName
        {
            get => _settings.GetDeviceCustomName(Id) ?? "";
            set
            {
                _settings.SetDeviceCustomName(Id, value);
                RaisePropertyChanged(nameof(CustomName));
            }
        }

        private readonly AppSettings _settings;

        public DeviceSettingsItemViewModel(AppSettings settings, string id, string originalName)
        {
            _settings = settings;
            Id = id;
            OriginalName = originalName;
        }
    }
}
