# Mixr test plan

Manual tests to run before a release. Tick each box as it passes. Add tests here whenever a feature is built.

## Status

| Feature | Status |
|---|---|
| Hide and rename devices | Passed 2026-10-21 |
| Cycle hotkey | Passed 2026-10-21 (core tests), extra checks pending |
| Auto switch on connect | Not tested yet |

## Setup (before every test run)

- [ ] Exit every Mixr (tray icon > right click > Exit), and check Task Manager for leftovers
- [ ] Start Mixr only with F5 in Visual Studio (Debug, x86, startup project `Mixr.Package`), or from the Start menu after a deploy. Never run `Build\Debug\Mixr.exe` (it has separate settings)
- [ ] Mixr icon appears in the tray

## 1. Hide and rename devices

- [ ] Settings > Devices lists all connected playback devices
- [ ] Untick **Show in Mixr** on a device: it disappears from the expanded flyout and the tray right click menu
- [ ] A hidden device is missing from an app's "move to device" menu (right click an app in the flyout)
- [ ] Hide the current default device: it still shows (default always shows)
- [ ] Right click a device in the flyout: the eye button hides it
- [ ] Right click the hidden default device: the button shows it again
- [ ] Type a **Custom name**, click out of the box: the new name shows in the flyout, tray menu and tray tooltip
- [ ] Clear the custom name: the original name comes back
- [ ] Exit and restart Mixr: hidden devices and names are kept

## 2. Cycle hotkey

Setup: Settings > Devices, tick **Include in cycle** on 2 or 3 devices. Settings > Shortcuts, set **Switch to the next device in the cycle** to an unused combo (e.g. Ctrl+Alt+F12, not Ctrl+J).

- [ ] Click into Notepad, press the shortcut several times: the default changes each time and wraps around
- [ ] The popup shows the right name each time, and sound really moves (play a video)
- [ ] Notepad keeps focus the whole time
- [ ] Press about 10 times very fast: no stutter, the final default matches the last name shown
- [ ] Press slowly, a second apart: steps through each ticked device
- [ ] Pick a different ticked device from the tray menu, then press the shortcut: it continues from the one you picked
- [ ] Untick every device and press the shortcut: nothing happens
- [ ] Rename a ticked device: the popup uses the new name
- [ ] A hidden device that is ticked is still part of the cycle
- [ ] Unplug or disable a ticked device: the cycle skips it
- [ ] Join a Discord (or other voice app) call and switch: the app's audio follows
- [ ] Exit and restart Mixr: ticks and the shortcut are kept

## 3. Auto switch on connect

Setup: with the headphones (or Quest Link) connected, Settings > Devices, tick **Switch to it when it connects…** for them. Make speakers the default from the tray menu.

- [ ] Unplug and replug the headphones: default switches to them, with the popup
- [ ] Unplug them: default goes back to the speakers, with the popup
- [ ] Override: plug in (it switches), pick the speakers from the tray menu, wait a few seconds, unplug: nothing switches, no popup
- [ ] Chain: speakers > headset > Quest. Disconnecting Quest returns to the headset, then unplugging the headset returns to the speakers
- [ ] Untick the box, plug in: nothing happens
- [ ] Devices already connected when Mixr starts do not trigger a switch
- [ ] Bluetooth headphones: turning them on and off behaves the same as plugging in
- [ ] Quest Link: starting and stopping Link switches to and from its audio device
- [ ] The setting stays saved after unplugging the device
- [ ] Cycle hotkey still works as before

## If something fails

Settings > About > open the diagnostic data and copy the lines starting with `DefaultDeviceCycler`, `DefaultDeviceSwitcher`, `AutoSwitchOnConnect` or `StorageFactory Using`.
