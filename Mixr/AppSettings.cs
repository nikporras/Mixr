using Mixr.DataModel.Storage;
using Mixr.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using static Mixr.Interop.User32;

namespace Mixr
{
    public class AppSettings
    {
        public event EventHandler<bool> UseLegacyIconChanged;
        public event Action FlyoutHotkeyTyped;
        public event Action MixerHotkeyTyped;
        public event Action SettingsHotkeyTyped;
        public event Action AbsoluteVolumeUpHotkeyTyped;
        public event Action AbsoluteVolumeDownHotkeyTyped;

        private ISettingsBag _settings = StorageFactory.GetSettings();

        public void RegisterHotkeys()
        {
            HotkeyManager.Current.Register(FlyoutHotkey);
            HotkeyManager.Current.Register(MixerHotkey);
            HotkeyManager.Current.Register(SettingsHotkey);
            HotkeyManager.Current.Register(AbsoluteVolumeUpHotkey);
            HotkeyManager.Current.Register(AbsoluteVolumeDownHotkey);

            HotkeyManager.Current.KeyPressed += (hotkey) =>
            {
                if (hotkey.Equals(FlyoutHotkey))
                {
                    Trace.WriteLine("AppSettings FlyoutHotkeyTyped");
                    FlyoutHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(SettingsHotkey))
                {
                    Trace.WriteLine("AppSettings SettingsHotkeyTyped");
                    SettingsHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(MixerHotkey))
                {
                    Trace.WriteLine("AppSettings MixerHotkeyTyped");
                    MixerHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(AbsoluteVolumeUpHotkey))
                {
                    Trace.WriteLine("AppSettings AbsoluteVolumeUpHotkeyTyped");
                    AbsoluteVolumeUpHotkeyTyped?.Invoke();
                }
                else if (hotkey.Equals(AbsoluteVolumeDownHotkey))
                {
                    Trace.WriteLine("AppSettings AbsoluteVolumeDownHotkeyTyped");
                    AbsoluteVolumeDownHotkeyTyped?.Invoke();
                }
            };
        }

        public HotkeyData FlyoutHotkey
        {
            get => _settings.Get("Hotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(FlyoutHotkey);
                _settings.Set("Hotkey", value);
                HotkeyManager.Current.Register(FlyoutHotkey);
            }
        }

        public HotkeyData MixerHotkey
        {
            get => _settings.Get("MixerHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(MixerHotkey);
                _settings.Set("MixerHotkey", value);
                HotkeyManager.Current.Register(MixerHotkey);
            }
        }

        public HotkeyData SettingsHotkey
        {
            get => _settings.Get("SettingsHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(SettingsHotkey);
                _settings.Set("SettingsHotkey", value);
                HotkeyManager.Current.Register(SettingsHotkey);
            }
        }

        public HotkeyData AbsoluteVolumeUpHotkey
        {
            get => _settings.Get("AbsoluteVolumeUpHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(AbsoluteVolumeUpHotkey);
                _settings.Set("AbsoluteVolumeUpHotkey", value);
                HotkeyManager.Current.Register(AbsoluteVolumeUpHotkey);
            }
        }

        public HotkeyData AbsoluteVolumeDownHotkey
        {
            get => _settings.Get("AbsoluteVolumeDownHotkey", new HotkeyData { });
            set
            {
                HotkeyManager.Current.Unregister(AbsoluteVolumeDownHotkey);
                _settings.Set("AbsoluteVolumeDownHotkey", value);
                HotkeyManager.Current.Register(AbsoluteVolumeDownHotkey);
            }
        }

        public bool UseLegacyIcon
        {
            get
            {
                // Note: Legacy compat, we used to write string bools.
                var ret = _settings.Get("UseLegacyIcon", "False");
                bool.TryParse(ret, out bool isUseLegacyIcon);
                return isUseLegacyIcon;
            }
            set
            {
                _settings.Set("UseLegacyIcon", value.ToString());
                UseLegacyIconChanged?.Invoke(null, UseLegacyIcon);
            }
        }

        public bool IsExpanded
        {
            get => _settings.Get("IsExpanded", false);
            set => _settings.Set("IsExpanded", value);
        }

        // Mixr: per-device preferences (hidden devices and friendly names).
        public event Action DevicePreferencesChanged;

        private string[] _hiddenDeviceIds;
        private DeviceNameEntry[] _deviceNames;

        private string[] HiddenDeviceIds
        {
            get
            {
                if (_hiddenDeviceIds == null)
                {
                    try
                    {
                        _hiddenDeviceIds = _settings.Get("HiddenDeviceIds", new string[] { }) ?? new string[] { };
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"AppSettings HiddenDeviceIds read failed: {ex}");
                        _hiddenDeviceIds = new string[] { };
                    }
                }
                return _hiddenDeviceIds;
            }
            set
            {
                _hiddenDeviceIds = value;
                _settings.Set("HiddenDeviceIds", value);
            }
        }

        private DeviceNameEntry[] DeviceNames
        {
            get
            {
                if (_deviceNames == null)
                {
                    try
                    {
                        _deviceNames = _settings.Get("DeviceNames", new DeviceNameEntry[] { }) ?? new DeviceNameEntry[] { };
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"AppSettings DeviceNames read failed: {ex}");
                        _deviceNames = new DeviceNameEntry[] { };
                    }
                }
                return _deviceNames;
            }
            set
            {
                _deviceNames = value;
                _settings.Set("DeviceNames", value);
            }
        }

        public bool IsDeviceHidden(string deviceId)
        {
            return !string.IsNullOrEmpty(deviceId) && HiddenDeviceIds.Contains(deviceId);
        }

        public void SetDeviceHidden(string deviceId, bool isHidden)
        {
            if (string.IsNullOrEmpty(deviceId) || IsDeviceHidden(deviceId) == isHidden)
            {
                return;
            }

            var ids = HiddenDeviceIds.Where(id => id != deviceId).ToList();
            if (isHidden)
            {
                ids.Add(deviceId);
            }
            HiddenDeviceIds = ids.ToArray();
            DevicePreferencesChanged?.Invoke();
        }

        public string GetDeviceCustomName(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return null;
            }
            return DeviceNames.FirstOrDefault(d => d.Id == deviceId)?.Name;
        }

        public void SetDeviceCustomName(string deviceId, string name)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return;
            }

            name = name?.Trim() ?? "";
            if ((GetDeviceCustomName(deviceId) ?? "") == name)
            {
                return;
            }

            var names = DeviceNames.Where(d => d.Id != deviceId).ToList();
            if (name.Length > 0)
            {
                names.Add(new DeviceNameEntry { Id = deviceId, Name = name });
            }
            DeviceNames = names.ToArray();
            DevicePreferencesChanged?.Invoke();
        }

        public bool UseScrollWheelInTray
        {
            get => _settings.Get("UseScrollWheelInTray", true);
            set => _settings.Set("UseScrollWheelInTray", value);
        }

        public bool UseGlobalMouseWheelHook
        {
            get => _settings.Get("UseGlobalMouseWheelHook", false);
            set => _settings.Set("UseGlobalMouseWheelHook", value);
        }

        public bool HasShownFirstRun
        {
            get => _settings.HasKey("hasShownFirstRun");
            set => _settings.Set("hasShownFirstRun", value);
        }

        public bool IsTelemetryEnabled
        {
            get
            {
                return _settings.Get("IsTelemetryEnabled", IsTelemetryEnabledByDefault());
            }
            set => _settings.Set("IsTelemetryEnabled", value);
        }

        public bool UseLogarithmicVolume
        {
            get => _settings.Get("UseLogarithmicVolume", false);
            set => _settings.Set("UseLogarithmicVolume", value);
        }

        public WINDOWPLACEMENT? FullMixerWindowPlacement
        {
            get => _settings.Get("FullMixerWindowPlacement", default(WINDOWPLACEMENT?));
            set => _settings.Set("FullMixerWindowPlacement", value);
        }

        public WINDOWPLACEMENT? SettingsWindowPlacement
        {
            get => _settings.Get("SettingsWindowPlacement", default(WINDOWPLACEMENT?));
            set => _settings.Set("SettingsWindowPlacement", value);
        }

        private bool IsTelemetryEnabledByDefault()
        {
            // Discussion on what to include:
            // https://gist.github.com/henrik/1688572
            var europeanUnionRegions = new string[]
            {
                // EU 28
                "AT", // Austria
                "BE", // Belgium
                "BG", // Bulgaria
                "HR", // Croatia
                "CY", // Cyprus
                "CZ", // Czech Republic
                "DK", // Denmark
                "EE", // Estonia
                "FI", // Finland
                "FR", // France
                "DE", // Germany
                "GR", // Greece
                "HU", // Hungary
                "IE", // Ireland, Republic of (EIRE)
                "IT", // Italy
                "LV", // Latvia
                "LT", // Lithuania
                "LU", // Luxembourg
                "MT", // Malta
                "NL", // Netherlands
                "PL", // Poland
                "PT", // Portugal
                "RO", // Romania
                "SK", // Slovakia
                "SI", // Slovenia
                "ES", // Spain
                "SE", // Sweden
                "GB", // United Kingdom (Great Britain)

                // Outermost Regions (OMR)
                "GF", // French Guiana
                "GP", // Guadeloupe
                "MQ", // Martinique
                "ME", // Montenegro
                "YT", // Mayotte
                "RE", // Réunion
                "MF", // Saint Martin

                // Special Cases: Part of EU
                "GI", // Gibraltar
                "AX", // Åland Islands

                // Overseas Countries and Territories (OCT)
                "PM", // Saint Pierre and Miquelon
                "GL", // Greenland
                "BL", // Saint Bartelemey
                "SX", // Sint Maarten
                "AW", // Aruba
                "CW", // Curacao
                "WF", // Wallis and Futuna
                "PF", // French Polynesia
                "NC", // New Caledonia
                "TF", // French Southern Territories
                "AI", // Anguilla
                "BM", // Bermuda
                "IO", // British Indian Ocean Territory
                "VG", // Virgin Islands, British
                "KY", // Cayman Islands
                "FK", // Falkland Islands (Malvinas)
                "MS", // Montserrat
                "PN", // Pitcairn
                "SH", // Saint Helena
                "GS", // South Georgia and the South Sandwich Islands
                "TC", // Turks and Caicos Islands

                // Microstates
                "AD", // Andorra
                "LI", // Liechtenstein
                "MC", // Monaco
                "SM", // San Marino
                "VA", // Vatican City

                // Other
                "JE", // Jersey
                "GG", // Guernsey
            };
            var region = new Windows.Globalization.GeographicRegion();
            return !europeanUnionRegions.Contains(region.CodeTwoLetter);
        }
    }

    // Mixr: a friendly name the user gave to an audio device.
    public class DeviceNameEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}