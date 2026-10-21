using Mixr.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Mixr.UI.ViewModels
{
    // Mixr: save the current audio setup as a profile, then apply it from here or from the tray menu.
    public class MixrProfilesSettingsPageViewModel : SettingsPageViewModel
    {
        public ObservableCollection<ProfileSettingsItemViewModel> Profiles { get; }
        public ICommand SaveCurrent { get; }

        private readonly AppSettings _settings;
        private readonly ProfileManager _profileManager;

        public MixrProfilesSettingsPageViewModel(AppSettings settings, ProfileManager profileManager) : base(null)
        {
            _settings = settings;
            _profileManager = profileManager;
            Title = Properties.Resources.ProfilesSettingsPageText;
            Glyph = "\xE8FD";

            Profiles = new ObservableCollection<ProfileSettingsItemViewModel>(
                settings.Profiles.Select(p => new ProfileSettingsItemViewModel(settings, profileManager, p, Remove)));
            SaveCurrent = new RelayCommand(OnSaveCurrent);
        }

        private void OnSaveCurrent()
        {
            var profile = new AudioProfile
            {
                Id = Guid.NewGuid().ToString(),
                Name = Properties.Resources.ProfileDefaultNameFormatText.Replace("{Number}", (_settings.Profiles.Length + 1).ToString()),
            };
            _profileManager.CaptureCurrent(profile);
            _settings.SaveProfile(profile);
            Profiles.Add(new ProfileSettingsItemViewModel(_settings, _profileManager, profile, Remove));
        }

        private void Remove(ProfileSettingsItemViewModel item)
        {
            _settings.DeleteProfile(item.Id);
            Profiles.Remove(item);
        }
    }

    public class ProfileSettingsItemViewModel : BindableBase
    {
        public string Id => _profile.Id;

        public string Name
        {
            get => _profile.Name;
            set
            {
                var name = value?.Trim() ?? "";
                if (name.Length == 0 || name == _profile.Name)
                {
                    RaisePropertyChanged(nameof(Name));
                    return;
                }
                _profile.Name = name;
                Save();
                RaisePropertyChanged(nameof(Name));
            }
        }

        public ObservableCollection<ProfileLineViewModel> Lines { get; } = new ObservableCollection<ProfileLineViewModel>();
        public bool IsEmpty => Lines.Count == 0;

        public ICommand Apply { get; }
        public ICommand ReplaceWithCurrent { get; }
        public ICommand Delete { get; }

        private readonly AppSettings _settings;
        private readonly AudioProfile _profile;

        public ProfileSettingsItemViewModel(AppSettings settings, ProfileManager profileManager, AudioProfile profile, Action<ProfileSettingsItemViewModel> remove)
        {
            _settings = settings;
            _profile = profile;

            Apply = new RelayCommand(() => profileManager.Apply(_profile));
            ReplaceWithCurrent = new RelayCommand(() =>
            {
                profileManager.CaptureCurrent(_profile);
                Save();
                RebuildLines();
            });
            Delete = new RelayCommand(() => remove(this));

            RebuildLines();
        }

        private void Save() => _settings.SaveProfile(_profile);

        private void RebuildLines()
        {
            Lines.Clear();

            if (!string.IsNullOrEmpty(_profile.OutputDeviceId))
            {
                Lines.Add(new ProfileLineViewModel(
                    Properties.Resources.ProfileOutputFormatText.Replace("{Name}", DeviceName(_profile.OutputDeviceName)),
                    () =>
                    {
                        _profile.OutputDeviceId = null;
                        _profile.OutputDeviceName = null;
                    },
                    OnLineRemoved));
            }

            if (!string.IsNullOrEmpty(_profile.InputDeviceId))
            {
                Lines.Add(new ProfileLineViewModel(
                    Properties.Resources.ProfileInputFormatText.Replace("{Name}", DeviceName(_profile.InputDeviceName)),
                    () =>
                    {
                        _profile.InputDeviceId = null;
                        _profile.InputDeviceName = null;
                    },
                    OnLineRemoved));
            }

            foreach (var route in _profile.AppRoutes ?? new AppRoute[] { })
            {
                var deviceText = string.IsNullOrEmpty(route.DeviceId) ? Properties.Resources.ProfileFollowDefaultText : DeviceName(route.DeviceName);
                Lines.Add(new ProfileLineViewModel(
                    Properties.Resources.ProfileAppRouteFormatText.Replace("{App}", route.AppName).Replace("{Device}", deviceText),
                    () => _profile.AppRoutes = _profile.AppRoutes.Where(r => r != route).ToArray(),
                    OnLineRemoved));
            }

            RaisePropertyChanged(nameof(IsEmpty));
        }

        private void OnLineRemoved()
        {
            Save();
            RebuildLines();
        }

        private static string DeviceName(string name) =>
            string.IsNullOrWhiteSpace(name) ? Properties.Resources.ProfileUnknownDeviceText : name;
    }

    public class ProfileLineViewModel
    {
        public string Text { get; }
        public ICommand Remove { get; }

        public ProfileLineViewModel(string text, Action removeFromProfile, Action onRemoved)
        {
            Text = text;
            Remove = new RelayCommand(() =>
            {
                removeFromProfile();
                onRemoved();
            });
        }
    }
}
