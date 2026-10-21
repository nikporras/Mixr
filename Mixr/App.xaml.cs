using Mixr.DataModel.WindowsAudio;
using Mixr.Diagnosis;
using Mixr.Extensibility;
using Mixr.Extensibility.Hosting;
using Mixr.Extensions;
using Mixr.Interop;
using Mixr.Interop.Helpers;
using Mixr.UI.Helpers;
using Mixr.UI.ViewModels;
using Mixr.UI.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Mixr
{
    public partial class App
    {
        public static bool IsShuttingDown { get; private set; }
        public static bool HasIdentity { get; private set; }
        public static bool HasDevIdentity { get; private set; }
        public static string PackageName { get; private set; }
        public static Version PackageVersion { get; private set; }
        public static TimeSpan Duration => s_appTimer.Elapsed;

        // Mixr: the packaged copy is the F5 dev build (the stable copy runs unpackaged from C:\Apps\Mixr).
        // It is red and called "Mixr Dev" so the two are easy to tell apart.
        public static bool IsDevBuild => HasIdentity;
        public static string AppDisplayName => IsDevBuild ? "Mixr Dev" : "Mixr";

        public FlyoutWindow FlyoutWindow { get; private set; }
        public DeviceCollectionViewModel CollectionViewModel { get; private set; }

        private static readonly Stopwatch s_appTimer = Stopwatch.StartNew();
        private FlyoutViewModel _flyoutViewModel;

        private ShellNotifyIcon _trayIcon;
        private WindowHolder _mixerWindow;
        private WindowHolder _settingsWindow;
        private DefaultDeviceSwitcher _defaultDeviceSwitcher;
        private DefaultDeviceCycler _deviceCycler;
        private AutoSwitchOnConnect _autoSwitchOnConnect;
        private ProfileManager _profileManager;
        private MicMuteController _micMuteController;
        private DeviceSwitchPopup _deviceSwitchPopup;
        private CommandChannel _commandChannel;
        private string[] _startupArgs = new string[] { };
        private ErrorReporter _errorReporter;

        public static AppSettings Settings { get; private set; }

        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            Exit += (_, __) => IsShuttingDown = true;
            HasIdentity = PackageHelper.CheckHasIdentity();
            HasDevIdentity = PackageHelper.HasDevIdentity();
            PackageVersion = PackageHelper.GetVersion(HasIdentity);
            PackageName = PackageHelper.GetFamilyName(HasIdentity);
            ImmersiveSystemColors.UseDevAccent = IsDevBuild;

            Settings = new AppSettings();
            _errorReporter = new ErrorReporter(Settings);

            if (SingleInstanceAppMutex.TakeExclusivity())
            {
                Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();
                // Mixr: a command (e.g. from a mixr:// link) that started Mixr runs once startup completes.
                _startupArgs = e.Args;

                try
                {
                    ContinueStartup();
                }
                catch (Exception ex) when (IsCriticalFontLoadFailure(ex))
                {
                    ErrorReporter.LogWarning(ex);
                    OnCriticalFontLoadFailure();
                }
            }
            else
            {
                // Mixr: another Mixr (packaged or not) is already running and keeps the tray icon.
                // Any command goes to it instead.
                if (e.Args.Length > 0)
                {
                    var sent = CommandChannel.Send(e.Args, TimeSpan.FromSeconds(10));
                    Trace.WriteLine($"App OnAppStartup Sent command to running instance: {sent}");
                }
                Trace.WriteLine("App OnAppStartup Another instance is running, exiting");
                Shutdown();
            }
        }

        private void ContinueStartup()
        {
            ((UI.Themes.Manager)Resources["ThemeManager"]).Load();

            var deviceManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);
            deviceManager.Loaded += (_, __) => CompleteStartup();
            CollectionViewModel = new DeviceCollectionViewModel(deviceManager, Settings);

            _trayIcon = new ShellNotifyIcon(new TaskbarIconSource(CollectionViewModel, Settings));
            Exit += (_, __) => _trayIcon.IsVisible = false;
            CollectionViewModel.TrayPropertyChanged += () => _trayIcon.SetTooltip(CollectionViewModel.GetTrayToolTip());

            _flyoutViewModel = new FlyoutViewModel(CollectionViewModel, () => _trayIcon.SetFocus(), Settings);
            FlyoutWindow = new FlyoutWindow(_flyoutViewModel);
            // Initialize the FlyoutWindow last because its Show/Hide cycle will pump messages, causing UI frames
            // to be executed, breaking the assumption that startup is complete.
            FlyoutWindow.Initialize();
        }

        private void CompleteStartup()
        {
            AddonManager.Load(shouldLoadInternalAddons: HasDevIdentity);
            Exit += (_, __) => AddonManager.Shutdown();
#if DEBUG
            DebugHelpers.Add();
#endif
            _mixerWindow = new WindowHolder(CreateMixerExperience);
            _settingsWindow = new WindowHolder(CreateSettingsExperience);

            // Mixr: the cycle hotkey and auto switch share one switcher so their requests coalesce instead of racing.
            _defaultDeviceSwitcher = new DefaultDeviceSwitcher();
            _deviceCycler = new DefaultDeviceCycler(CollectionViewModel, Settings, _defaultDeviceSwitcher);
            _autoSwitchOnConnect = new AutoSwitchOnConnect(CollectionViewModel, Settings, _defaultDeviceSwitcher);
            _autoSwitchOnConnect.Switched += ShowDeviceSwitchPopup;

            // Mixr: profiles also set the default microphone, which needs its own switcher so the two don't coalesce.
            _profileManager = new ProfileManager(
                CollectionViewModel,
                WindowsAudioFactory.Create(AudioDeviceKind.Recording),
                _defaultDeviceSwitcher,
                new DefaultDeviceSwitcher());
            _profileManager.Applied += profile => ShowSwitchPopup(Mixr.Properties.Resources.ProfileAppliedFormatText.Replace("{Name}", profile.Name));

            _micMuteController = new MicMuteController(WindowsAudioFactory.Create(AudioDeviceKind.Recording), Settings);

            Settings.FlyoutHotkeyTyped += () => _flyoutViewModel.OpenFlyout(InputType.Keyboard);
            Settings.MixerHotkeyTyped += () => _mixerWindow.OpenOrClose();
            Settings.SettingsHotkeyTyped += () => _settingsWindow.OpenOrBringToFront();
            Settings.AbsoluteVolumeUpHotkeyTyped += AbsoluteVolumeIncrement;
            Settings.AbsoluteVolumeDownHotkeyTyped += AbsoluteVolumeDecrement;
            Settings.CycleDeviceHotkeyTyped += CycleDefaultDevice;
            Settings.MicMuteHotkeyTyped += ToggleMicMute;
            Settings.RegisterHotkeys();

            _trayIcon.PrimaryInvoke += (_, type) => _flyoutViewModel.OpenFlyout(type);
            _trayIcon.SecondaryInvoke += (_, args) => _trayIcon.ShowContextMenu(GetTrayContextMenuItems(), args.Point);
            _trayIcon.TertiaryInvoke += (_, __) => CollectionViewModel.Default?.ToggleMute.Execute(null);
            _trayIcon.Scrolled += trayIconScrolled;
            _trayIcon.SetTooltip(CollectionViewModel.GetTrayToolTip());
            _trayIcon.IsVisible = true;

            // Mixr: Mixr.exe arguments and mixr:// links, from later launches and the one that started Mixr.
            _commandChannel = new CommandChannel(RunCommand);
            Exit += (_, __) => _commandChannel.Dispose();
            if (!HasIdentity)
            {
                ProtocolRegistration.RegisterForCurrentUser(MixrCommand.UriScheme);
            }

            DisplayFirstRunExperience();

            if (_startupArgs.Length > 0)
            {
                RunCommand(_startupArgs);
            }
        }

        private void trayIconScrolled(object _, int wheelDelta)
        {
            if (Settings.UseScrollWheelInTray && (!Settings.UseGlobalMouseWheelHook || _flyoutViewModel.State == FlyoutViewState.Hidden))
            {
                var hWndTray = WindowsTaskbar.GetTrayToolbarWindowHwnd();
                var hWndTooltip = User32.SendMessage(hWndTray, User32.TB_GETTOOLTIPS, IntPtr.Zero, IntPtr.Zero);
                User32.SendMessage(hWndTooltip, User32.TTM_POPUP, IntPtr.Zero, IntPtr.Zero);
                
                CollectionViewModel.Default?.IncrementVolume(Math.Sign(wheelDelta) * 2);
            }
        }

        private void DisplayFirstRunExperience()
        {
            if (!Settings.HasShownFirstRun
#if DEBUG
                || Keyboard.IsKeyDown(Key.LeftCtrl)
#endif
                )
            {
                Trace.WriteLine($"App DisplayFirstRunExperience Showing welcome dialog");
                Settings.HasShownFirstRun = true;

                var dialog = new DialogWindow { DataContext = new WelcomeViewModel(Settings) };
                dialog.Show();
                dialog.RaiseWindow();
            }
        }

        private bool IsCriticalFontLoadFailure(Exception ex)
        {
            return ex.StackTrace.Contains("MS.Internal.Text.TextInterface.FontFamily.GetFirstMatchingFont") ||
                   ex.StackTrace.Contains("MS.Internal.Text.Line.Format");
        }

        private void OnCriticalFontLoadFailure()
        {
            Trace.WriteLine($"App OnCriticalFontLoadFailure");

            new Thread(() =>
            {
                if (MessageBox.Show(
                    Mixr.Properties.Resources.CriticalFailureFontLookupHelpText,
                    Mixr.Properties.Resources.CriticalFailureDialogHeaderText,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    Trace.WriteLine($"App OnCriticalFontLoadFailure OK");
                    ProcessHelper.StartNoThrow("https://eartrumpet.app/jmp/fixfonts");
                }
                Environment.Exit(0);
            }).Start();

            // Stop execution because callbacks to the UI thread will likely cause another cascading font error.
            new AutoResetEvent(false).WaitOne();
        }

        private IEnumerable<ContextMenuItem> GetTrayContextMenuItems()
        {
            var ret = new List<ContextMenuItem>(CollectionViewModel.AllDevices.Where(x => !x.IsHidden || x.Id == CollectionViewModel.Default?.Id).OrderBy(x => x.DisplayName).Select(dev => new ContextMenuItem
            {
                DisplayName = dev.DisplayName,
                IsChecked = dev.Id == CollectionViewModel.Default?.Id,
                Command = new RelayCommand(() => dev.MakeDefaultDevice()),
            }));

            if (!ret.Any())
            {
                ret.Add(new ContextMenuItem
                {
                    DisplayName = Mixr.Properties.Resources.ContextMenuNoDevices,
                    IsEnabled = false,
                });
            }

            // Mixr: one click applies a saved profile.
            var profiles = Settings.Profiles;
            if (profiles.Any())
            {
                ret.Add(new ContextMenuSeparator());
                ret.Add(new ContextMenuItem
                {
                    DisplayName = Mixr.Properties.Resources.ProfilesMenuText,
                    Children = profiles.Select(profile => new ContextMenuItem
                    {
                        DisplayName = profile.Name,
                        Command = new RelayCommand(() => _profileManager.Apply(profile)),
                    }).ToList(),
                });
            }

            ret.AddRange(new List<ContextMenuItem>
                {
                    new ContextMenuSeparator(),
                    new ContextMenuItem
                    {
                        DisplayName = Mixr.Properties.Resources.WindowsLegacyMenuText,
                        Children = new List<ContextMenuItem>
                        {
                            new ContextMenuItem { DisplayName = Mixr.Properties.Resources.LegacyVolumeMixerText, Command =  new RelayCommand(LegacyControlPanelHelper.StartLegacyAudioMixer) },
                            new ContextMenuItem { DisplayName = Mixr.Properties.Resources.PlaybackDevicesText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("playback")) },
                            new ContextMenuItem { DisplayName = Mixr.Properties.Resources.RecordingDevicesText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("recording")) },
                            new ContextMenuItem { DisplayName = Mixr.Properties.Resources.SoundsControlPanelText, Command = new RelayCommand(() => LegacyControlPanelHelper.Open("sounds")) },
                            new ContextMenuItem { DisplayName = Mixr.Properties.Resources.OpenSoundSettingsText, Command = new RelayCommand(() => SettingsPageHelper.Open("sound")) },
                            new ContextMenuItem {
                                DisplayName = Environment.OSVersion.IsAtLeast(OSVersions.Windows11) ?
                                    Mixr.Properties.Resources.OpenAppsVolume_Windows11_Text
                                    : Mixr.Properties.Resources.OpenAppsVolume_Windows10_Text, Command = new RelayCommand(() => SettingsPageHelper.Open("apps-volume")) },
                        },
                    },
                    new ContextMenuSeparator(),
                });

            var addonItems = AddonManager.Host.TrayContextMenuItems?.OrderBy(x => x.NotificationAreaContextMenuItems.FirstOrDefault()?.DisplayName).SelectMany(ext => ext.NotificationAreaContextMenuItems);
            if (addonItems != null && addonItems.Any())
            {
                ret.AddRange(addonItems);
                ret.Add(new ContextMenuSeparator());
            }

            ret.AddRange(new List<ContextMenuItem>
                {
                    new ContextMenuItem { DisplayName = Mixr.Properties.Resources.FullWindowTitleText, Command = new RelayCommand(_mixerWindow.OpenOrBringToFront) },
                    new ContextMenuItem { DisplayName = Mixr.Properties.Resources.SettingsWindowText, Command = new RelayCommand(_settingsWindow.OpenOrBringToFront) },
                    new ContextMenuItem { DisplayName = Mixr.Properties.Resources.ContextMenuExitTitle, Command = new RelayCommand(Shutdown) },
                });
            return ret;
        }

        private Window CreateSettingsExperience()
        {
            var defaultCategory = new SettingsCategoryViewModel(
                Mixr.Properties.Resources.SettingsCategoryTitle,
                "\xE71D",
                Mixr.Properties.Resources.SettingsDescriptionText,
                null,
                new SettingsPageViewModel[]
                    {
                        new MixrDevicesSettingsPageViewModel(Settings, CollectionViewModel),
                        new MixrProfilesSettingsPageViewModel(Settings, _profileManager),
                        new MixrShortcutsPageViewModel(Settings),
                        new MixrMouseSettingsPageViewModel(Settings),
                        new MixrCommunitySettingsPageViewModel(Settings),
                        new MixrLegacySettingsPageViewModel(Settings),
                        new MixrAboutPageViewModel(() => _errorReporter.DisplayDiagnosticData(), Settings)
                    });

            var allCategories = new List<SettingsCategoryViewModel>();
            allCategories.Add(defaultCategory);

            if (AddonManager.Host.SettingsItems != null)
            {
                allCategories.AddRange(AddonManager.Host.SettingsItems.Select(a => CreateAddonSettingsPage(a)));
            }

            // Mixr: the dev build's title says so.
            var title = IsDevBuild ? $"{Mixr.Properties.Resources.SettingsWindowText} - {AppDisplayName}" : Mixr.Properties.Resources.SettingsWindowText;
            var viewModel = new SettingsViewModel(title, allCategories);
            return new SettingsWindow { DataContext = viewModel };
        }

        private SettingsCategoryViewModel CreateAddonSettingsPage(IMixrAddonSettingsPage addonSettingsPage)
        {
            var addon = (MixrAddon)addonSettingsPage;
            var category = addonSettingsPage.GetSettingsCategory();

            if (!addon.IsInternal())
            {
                category.Pages.Add(new AddonAboutPageViewModel(addon));
            }
            return category;
        }

        private Window CreateMixerExperience() => new FullWindow { DataContext = new FullWindowViewModel(CollectionViewModel) };

        // Mixr: make the next device ticked "Include in cycle" the default output.
        private void CycleDefaultDevice()
        {
            try
            {
                var next = _deviceCycler.Next();
                if (next != null)
                {
                    ShowDeviceSwitchPopup(next);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"App CycleDefaultDevice Failed: {ex}");
                ErrorReporter.LogWarning(ex);
            }
        }

        // Mixr: runs a command from Mixr.exe arguments or a mixr:// link (see MixrCommand).
        private void RunCommand(string[] args)
        {
            try
            {
                var command = MixrCommand.Parse(args);
                if (command == null)
                {
                    Trace.WriteLine($"App RunCommand Unknown command: {string.Join(" ", args)}");
                    return;
                }

                Trace.WriteLine($"App RunCommand {command}");
                switch (command.Kind)
                {
                    case MixrCommandKind.Device:
                        SwitchToDevice(command.Argument);
                        break;
                    case MixrCommandKind.Cycle:
                        CycleDefaultDevice();
                        break;
                    case MixrCommandKind.Profile:
                        ApplyProfile(command.Argument);
                        break;
                    case MixrCommandKind.Mic:
                        MixrCommand.TryGetMicMute(command.Argument, out var mute);
                        SetMicMute(mute);
                        break;
                    case MixrCommandKind.Volume:
                        SetDefaultDeviceVolume(command.Argument);
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"App RunCommand Failed: {ex}");
                ErrorReporter.LogWarning(ex);
            }
        }

        private void SwitchToDevice(string nameOrId)
        {
            var device = MixrCommand.FindDevice(CollectionViewModel.AllDevices, nameOrId);
            if (device == null)
            {
                Trace.WriteLine($"App SwitchToDevice No device matches '{nameOrId}'");
                ShowSwitchPopup(Mixr.Properties.Resources.CommandNotFoundFormatText.Replace("{Name}", nameOrId));
                return;
            }

            _deviceCycler.SetPosition(device.Id);
            _defaultDeviceSwitcher.RequestDefault(device.Id);
            ShowDeviceSwitchPopup(device);
        }

        private void ApplyProfile(string name)
        {
            var profile = Settings.Profiles.FirstOrDefault(p => string.Equals(p.Name?.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                Trace.WriteLine($"App ApplyProfile No profile named '{name}'");
                ShowSwitchPopup(Mixr.Properties.Resources.CommandNotFoundFormatText.Replace("{Name}", name));
                return;
            }

            // ProfileManager.Applied shows the popup.
            _profileManager.Apply(profile);
        }

        private void SetDefaultDeviceVolume(string argument)
        {
            var device = CollectionViewModel.Default;
            if (device == null || !MixrCommand.TryGetVolume(argument, device.Volume, out var volume))
            {
                return;
            }

            device.Volume = volume;
            ShowSwitchPopup(Mixr.Properties.Resources.DeviceVolumeFormatText
                .Replace("{Name}", device.DisplayName)
                .Replace("{Volume}", volume.ToString()));
        }

        // Mixr: mute or unmute the default microphone and say which it is now.
        private void ToggleMicMute() => SetMicMute(null);

        // Mutes (true), unmutes (false) or toggles (null) the default microphone.
        private void SetMicMute(bool? mute)
        {
            try
            {
                var isMuted = _micMuteController.SetMute(mute);
                if (isMuted == null)
                {
                    ShowSwitchPopup(Mixr.Properties.Resources.NoMicrophoneText, DeviceSwitchPopup.MicrophoneOffGlyph);
                }
                else if (isMuted.Value)
                {
                    ShowSwitchPopup(Mixr.Properties.Resources.MicMutedText, DeviceSwitchPopup.MicrophoneOffGlyph);
                }
                else
                {
                    ShowSwitchPopup(Mixr.Properties.Resources.MicUnmutedText, DeviceSwitchPopup.MicrophoneGlyph);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"App SetMicMute Failed: {ex}");
                ErrorReporter.LogWarning(ex);
            }
        }

        private void ShowDeviceSwitchPopup(DeviceViewModel device) => ShowSwitchPopup(device.DisplayName);

        private void ShowSwitchPopup(string text) => ShowSwitchPopup(text, DeviceSwitchPopup.SpeakerGlyph);

        private void ShowSwitchPopup(string text, string glyph)
        {
            try
            {
                if (_deviceSwitchPopup == null)
                {
                    _deviceSwitchPopup = new DeviceSwitchPopup();
                }
                _deviceSwitchPopup.ShowText(text, glyph);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"App ShowSwitchPopup Failed: {ex}");
                ErrorReporter.LogWarning(ex);
            }
        }

        private void AbsoluteVolumeIncrement()
        {
            foreach (var device in CollectionViewModel.AllDevices.Where(d => !d.IsMuted || d.IsAbsMuted))
            {
                // in any case this device is not abs muted anymore
                device.IsAbsMuted = false;
                device.IncrementVolume(2);
            }
        }

        private void AbsoluteVolumeDecrement()
        {
            foreach (var device in CollectionViewModel.AllDevices.Where(d => !d.IsMuted))
            {
                // if device is not muted but will be muted by 
                bool wasMuted = device.IsMuted;
                // device.IncrementVolume(-2);
                device.Volume -= 2;
                // if device is muted by this absolute down
                // .IsMuted is not already updated
                if (!wasMuted == (device.Volume <= 0))
                {
                    device.IsAbsMuted = true;
                }
            }
        }
    }
}
