# Mixr

Windows tray app for volume and audio device control. Fork of EarTrumpet (MIT), renamed to Mixr throughout. C#, WPF, .NET Framework 4.6.2, packaged as MSIX. Owner: nikporras.

## Build

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" Mixr.sln -restore -p:RestorePackagesConfig=true -p:Configuration=Debug -p:Platform=x86
```

- Always build `x86`. Fix every build error before handing back.
- To run: open `Mixr.sln` in Visual Studio 2022, startup project `Mixr.Package`, press F5 (Deploy must be ticked in Configuration Manager). The user does UI testing; tell them exactly what to click to test.
- Targets Windows SDK 10.0.26100.0 (set in `Mixr/Mixr.csproj` and `Mixr.Package/Mixr.Package.wapproj`).
- Each build bumps the version in `Mixr.Package/Package.appxmanifest`. Don't commit that change unless asked; `git restore` it.
- Settings: the packaged app uses package LocalSettings (`%LOCALAPPDATA%\Packages\nikporras.Mixr_1sdd7yawvg6ne`), but running `Build\Debug\Mixr.exe` directly uses `HKCU\Software\Mixr`, so settings look lost. Only one Mixr runs at a time; a stray unpackaged instance makes a packaged launch exit and also locks `Build\Debug\Mixr.exe` for the build.
- `PublisherDisplayName` must be the same in `Mixr.Package/Package.appxmanifest` and `Mixr.Package/Package.StoreAssociation.xml` (currently `nikporras`), or the package build fails with APPX1603.

## Daily use version

- `publish-stable.ps1` closes any running Mixr, builds Release x86 (`Mixr/Mixr.csproj` only, so the package version isn't bumped), mirrors `Build\Release` to `C:\Apps\Mixr`, creates `Mixr.lnk` in the Startup folder and starts it. Don't run it unless asked.
- Stable: `C:\Apps\Mixr\Mixr.exe`, unpackaged, settings in `HKCU\Software\Mixr`. Dev: F5 on `Mixr.Package`, packaged LocalSettings. Separate settings.
- Only one Mixr runs at a time: if the stable one is running, an F5 launch exits. Quit it from the tray first.

## Layout

- `Mixr/` main app
  - `App.xaml.cs` startup, tray context menu, settings window pages
  - `AppSettings.cs` all settings (registry or packaged app storage via `ISettingsBag`, values XML-serialized)
  - `DataModel/` Windows Core Audio wrappers (devices, sessions)
  - `UI/ViewModels/` MVVM view models; `UI/Views/` XAML
  - `UI/ViewModels/FlyoutViewModel.cs` the tray flyout device list
  - `UI/ViewModels/DeviceCollectionViewModel.cs` all playback devices, default device
  - `Addons/Mixr.Actions/` built-in Actions addon (triggers like hotkey or app start, actions like set default device)
  - `Properties/Resources.resx` UI strings (+ translations `Resources.xx-XX.resx`)
- `Mixr.Package/` MSIX packaging project
- `Mixr.ColorTool/` dev tool for theme colours

## Conventions

- Files are UTF-8 with BOM and CRLF line endings. Keep both.
- Old-style csproj: every new `.cs` needs a `<Compile Include=...>` and every new `.xaml` a `<Page Include=...>` in `Mixr/Mixr.csproj`.
- New UI strings: add to `Properties/Resources.resx` and a matching property in `Properties/Resources.Designer.cs`. English only is fine; other languages fall back to English.
- Settings pages: a `SettingsPageViewModel` subclass, a `DataTemplate` in `UI/Views/SettingsWindow.xaml`, and registration in `App.xaml.cs` `CreateSettingsExperience`.
- Mark Mixr-specific changes in upstream code with a short `// Mixr:` comment.
- Keep the original EarTrumpet notice in `LICENSE`.
- Commit messages: short summary line, then bullet points of what changed.
- After each feature: add its manual test steps as a new numbered section in `TESTPLAN.md` (checkbox list, setup line first) and add a row to its Status table with "Not tested yet". Only the user changes a status to passed.

## Done

- Rename to Mixr, modern SDK, own package identity (`nikporras.Mixr`)
- Hide and rename playback devices: `AppSettings` (`IsDeviceHidden`, `SetDeviceHidden`, `GetDeviceCustomName`, `SetDeviceCustomName`, `DevicePreferencesChanged`), Settings > Devices page (`MixrDevicesSettingsPageViewModel`), right-click Hide/Show button on devices (`FocusedDeviceViewModel`). Hidden devices are left out of the flyout, tray menu and app "move to device" menu; the current default always shows.
- Cycle hotkey: "Include in cycle" tick box per device on the Devices page (`AppSettings.IsDeviceInCycle`/`SetDeviceInCycle`, stored as `CycleDeviceIds`), shortcut on the Shortcuts page (`CycleDeviceHotkey`, no default). `UI/Helpers/DefaultDeviceCycler` keeps its own cycle position (reset when the default changes some other way); `UI/Views/DeviceSwitchPopup` shows its name for 1.5 s, bottom centre, without taking focus.
- Auto switch on connect: "Switch to it when it connects…" tick box per device (`AppSettings.IsDeviceAutoSwitch`, stored as `AutoSwitchDeviceIds`). `UI/Helpers/AutoSwitchOnConnect` watches `AllDevices` adds/removes, remembers the previous default, and switches back on disconnect unless the user picked another device more than 2 s before it (Windows' own default change on disconnect is ignored). Shows `DeviceSwitchPopup` too.
- `UI/Helpers/DefaultDeviceSwitcher` is the one place that sets default devices (cycle hotkey, auto switch, profiles): off the UI thread, coalescing to the latest request, failures logged. Use it rather than `DeviceViewModel.MakeDefaultDevice` for anything automatic or rapid. Each request carries its roles: the cycle hotkey and auto switch use `DefaultRoles` (`eMultimedia`), profiles use `AllRoles` (default and default communications device) for both output and mic.
- Profiles (built natively, not in the Actions addon, which only loads for dev identity): `AudioProfile` / `AppRoute` in `AppSettings.cs` (stored as `Profiles`), `UI/Helpers/ProfileManager` captures the current setup (default output, default mic via `WindowsAudioFactory.Create(Recording)`, every running movable app's persisted route) and applies it (app routes first, then defaults; missing devices and apps that aren't running are skipped and logged). Settings > Profiles page (`MixrProfilesSettingsPageViewModel`): save current setup, rename, remove lines, replace with current, apply, delete. Tray right click > Profiles submenu applies one. Profiles are edited by capture, not device pickers, because the app's implicit `ComboBox` style is the editable search box.
- Mic mute hotkey: `MicMuteHotkey` (no default) and `ShowMicMuteIndicator` (default on) on the Shortcuts page. `UI/Helpers/MicMuteController` toggles mute on the default recording device and follows its `IsMuted` (whoever changes it) and default changes, showing `UI/Views/MicMuteIndicator` (top right badge) while muted. The hotkey also shows `DeviceSwitchPopup` with a mic glyph (`ShowText(text, glyph)`). Overlays use `WindowExtensions.MakeClickThroughOverlay`.

- Command line and `mixr://` actions: `UI/Helpers/MixrCommand` parses `Mixr.exe <verb> <arg>` and `mixr://<verb>/<arg>` (device, cycle, profile, mic, volume) and finds devices by id/name. A second Mixr.exe sends its arguments to the running one through `Interop/Helpers/CommandChannel` (message-only window, `WM_COPYDATA`) and exits; arguments that started Mixr run after `CompleteStartup`. `App.RunCommand` runs them with the same popups as the hotkeys. `Interop/Helpers/ProtocolRegistration` registers `mixr://` in `HKCU\Software\Classes` from the unpackaged Mixr only; the package deliberately doesn't declare the protocol, so two handlers never compete.

## Roadmap (in order)

1. Later: Equalizer APO preset switcher, hardware sliders, active profile: route apps when they launch.
