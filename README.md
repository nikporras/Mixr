# Mixr

Volume and device control for Windows. A personal fork of [EarTrumpet](https://github.com/File-New-Project/EarTrumpet), reworked for setups with lots of audio devices.

## Features

- **Hide and rename devices**: choose which playback devices show in the flyout and tray menu, and give them friendly names (Settings > Devices). The current default always shows.
- **Cycle hotkey**: tick "Include in cycle" on the devices you switch between, set a shortcut (Settings > Shortcuts), and each press makes the next one the default output. A small popup shows the new device's name.
- **Auto switch on connect**: tick a device (headphones, Quest Link) and Mixr makes it the default when it connects, then switches back to the previous device when it disconnects, unless you picked another one meanwhile.
- **Profiles**: save the current default output, default microphone and where each app plays as a profile (Settings > Profiles), then apply it in one click from the tray icon's right-click menu. Applying sets both the default and the default communications device.
- **Mic mute hotkey with indicator**: a shortcut mutes or unmutes the default microphone, and a small badge stays in the top right corner while it's muted, however it was muted.

## Roadmap

- Command line / `mixr://` actions for Stream Deck
- Equalizer APO preset switcher
- Hardware sliders
- Active profile: route apps when they launch

## Building

See [COMPILING.md](COMPILING.md).

## Credits

Mixr is built on EarTrumpet by File-New-Project and its contributors and translators, used under the MIT license. See [LICENSE](LICENSE). The upstream changelog is kept in [CHANGELOG.md](CHANGELOG.md).
