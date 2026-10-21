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

## Done

- Rename to Mixr, modern SDK, own package identity (`nikporras.Mixr`)
- Hide and rename playback devices: `AppSettings` (`IsDeviceHidden`, `SetDeviceHidden`, `GetDeviceCustomName`, `SetDeviceCustomName`, `DevicePreferencesChanged`), Settings > Devices page (`MixrDevicesSettingsPageViewModel`), right-click Hide/Show button on devices (`FocusedDeviceViewModel`). Hidden devices are left out of the flyout, tray menu and app "move to device" menu; the current default always shows.

## Roadmap (in order)

1. **Cycle hotkey**: "Include in cycle" tick box per device on the Devices page; configurable shortcut on the Shortcuts page (no default); each press makes the next ticked device the default output; small popup shows the new device name for a second or two.
2. **Auto switch on connect**: when a chosen device appears (headphones, Quest Link), make it default; switch back when it disconnects.
3. **Profiles**: one click sets default output, default mic and per-app routing together (can build on the Actions addon).
4. Later: mic mute hotkey with indicator, command line / `mixr://` actions for Stream Deck, Equalizer APO preset switcher, hardware sliders.
