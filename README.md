# Mixr

Volume and device control for Windows. A personal fork of [EarTrumpet](https://github.com/File-New-Project/EarTrumpet), reworked for setups with lots of audio devices.

## Features

- **Hide and rename devices**: choose which playback devices show in the flyout and tray menu, and give them friendly names (Settings > Devices). The current default always shows.
- **Cycle hotkey**: tick "Include in cycle" on the devices you switch between, set a shortcut (Settings > Shortcuts), and each press makes the next one the default output. A small popup shows the new device's name.
- **Auto switch on connect**: tick a device (headphones, Quest Link) and Mixr makes it the default when it connects, then switches back to the previous device when it disconnects, unless you picked another one meanwhile.
- **Profiles**: save the current default output, default microphone and where each app plays as a profile (Settings > Profiles), then apply it in one click from the tray icon's right-click menu. Applying sets both the default and the default communications device. The last applied profile stays active (ticked in the menu): when one of its apps starts later, Mixr moves it to its saved device. Pick **None** to stop that.
- **Mic mute hotkey with indicator**: a shortcut mutes or unmutes the default microphone, and a small badge stays in the top right corner while it's muted, however it was muted.
- **Command line and `mixr://` actions**: control Mixr from Stream Deck, AutoHotkey or a shortcut. See [Commands](#commands).

## Commands

Run `Mixr.exe` with a command, or open a `mixr://` link. The command goes to the Mixr that's already running (stable or dev), so no second copy starts. If Mixr isn't running, it starts and then runs the command.

| What | Command line | Link |
|---|---|---|
| Make a device the default output | `Mixr.exe device "Headphones"` | `mixr://device/Headphones` |
| Next device in the cycle | `Mixr.exe cycle` | `mixr://cycle` |
| Apply a profile | `Mixr.exe profile "Gaming"` | `mixr://profile/Gaming` |
| Toggle, mute or unmute the mic | `Mixr.exe mic toggle` / `mute` / `unmute` | `mixr://mic/toggle` |
| Set the default device volume | `Mixr.exe volume 40` | `mixr://volume/40` |
| Change it | `Mixr.exe volume +10`, `volume -10`, `volume up`, `volume down 2` | `mixr://volume/up/10` |

- **Devices** match by Mixr's custom name, the Windows name ("Speakers (Realtek Audio)"), the short name ("Speakers") or the device id, ignoring case. Part of a name works if only one device has it.
- **Profiles** match by name, ignoring case. `profile none` (`mixr://profile/none`) clears the active profile, unless you have a profile called None.
- **Volume** is 0 to 100. `up` and `down` step by 5 unless you give a number. Use `up`/`down` in links, since some apps turn `+` into a space.
- Names with spaces need quotes on the command line and `%20` in links: `mixr://profile/Late%20Night`.
- Each command shows the same popup as the hotkeys. A device or profile that doesn't exist shows "Not found".

`mixr://` links are registered by the stable version (`C:\Apps\Mixr`, see below) when it starts. They also reach the dev version if that's the one running.

Examples:

- **Stream Deck**: System > Open, `C:\Apps\Mixr\Mixr.exe cycle`. System > Website with `mixr://profile/Gaming` also works, but goes through the browser, so Open is cleaner.
- **AutoHotkey v2**: `^!F10::Run 'C:\Apps\Mixr\Mixr.exe volume up'`
- **Shortcut on the desktop**: target `C:\Apps\Mixr\Mixr.exe device "Speakers"`.

## Roadmap

- Equalizer APO preset switcher
- Hardware sliders

## Building

See [COMPILING.md](COMPILING.md).

## Daily use version

Run `.\publish-stable.ps1` to install a stable copy for everyday use. It closes any running Mixr, builds Release x86, copies it to `C:\Apps\Mixr`, adds a shortcut to the Startup folder so it starts at login, and starts it.

- **Stable**: `C:\Apps\Mixr\Mixr.exe`, unpackaged, settings in the registry (`HKCU\Software\Mixr`).
- **Dev**: F5 in Visual Studio (`Mixr.Package`), packaged, settings in the package's LocalSettings.

The two have separate settings. Only one Mixr can run at a time, so quit the stable one from the tray before pressing F5, and start it again afterwards.

## Credits

Mixr is built on EarTrumpet by File-New-Project and its contributors and translators, used under the MIT license. See [LICENSE](LICENSE). The upstream changelog is kept in [CHANGELOG.md](CHANGELOG.md).
