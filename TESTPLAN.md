# Mixr test plan

Manual tests to run before a release. Tick each box as it passes. Add tests here whenever a feature is built.

## Status

| Feature | Status |
|---|---|
| Hide and rename devices | Passed 2026-10-21 |
| Cycle hotkey | Passed 2026-10-21 (core tests), extra checks pending |
| Auto switch on connect | Not tested yet |
| Profiles | Not tested yet |
| Mic mute hotkey | Not tested yet |
| Daily use version | Not tested yet |
| Command line and mixr:// actions | Not tested yet |
| Dev build look | Not tested yet |

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

## 4. Profiles

Setup: open two apps that play sound (e.g. Spotify and a browser video) so they show in the flyout. Have at least two playback devices and two microphones.

- [ ] Settings > Profiles shows the description and a **Save current setup as a new profile** button
- [ ] Set up "A": speakers default, mic 1 default, move Spotify to the headset (flyout > right click Spotify > move). Click **Save current setup…**: "Profile 1" appears listing Default output: speakers, Default microphone: mic 1, Spotify: headset, the browser: follows the default output
- [ ] Rename it to "Work", click out of the box: the name sticks
- [ ] Set up "B": headset default, mic 2 default, Spotify back to the default device. Save it, rename it "Gaming"
- [ ] Tray right click > **Profiles** lists Work and Gaming
- [ ] Click Work: the popup shows "Profile: Work", the default output and mic change (check Windows Sound settings), Spotify plays on the headset
- [ ] Click Gaming: the default output and mic switch, Spotify follows the default again
- [ ] Windows Sound control panel (tray right click > Windows legacy > Playback/Recording devices): the profile's output and mic are both "Default Device" and "Default Communication Device"
- [ ] A voice app (Discord) set to "Default" input picks up the new mic
- [ ] The **Apply** button on the Profiles page does the same as the tray menu
- [ ] Remove a line with its × button (e.g. the mic): it's gone, and applying no longer changes the mic
- [ ] Remove every line: "This profile doesn't change anything." shows, and applying changes nothing
- [ ] **Replace with current setup** refreshes the lines to match what's set now
- [ ] Close Spotify, apply a profile that routes it: nothing breaks, the other parts still apply
- [ ] Unplug a device a profile uses, apply it: that part is skipped, the rest still applies
- [ ] **Delete** removes the profile from the page and the tray menu
- [ ] With no profiles, the tray menu has no Profiles entry
- [ ] Exit and restart Mixr: profiles, names and lines are kept
- [ ] Cycle hotkey and auto switch still work as before

## 5. Mic mute hotkey

Setup: Settings > Shortcuts, set **Mute or unmute the default microphone** to an unused combo (e.g. Ctrl+Alt+F11). **Show an indicator…** is ticked by default.

- [ ] Click into Notepad, press the shortcut: popup says "Microphone muted", a "Microphone muted" badge appears in the top right corner, Notepad keeps focus
- [ ] Windows Sound settings shows the default mic as muted; a voice app (Discord) stops picking you up
- [ ] Press again: popup says "Microphone on", the badge disappears, the mic works again
- [ ] The badge doesn't block clicks: click a button underneath it
- [ ] Mute the mic from Windows Sound settings instead: the badge appears; unmute there: it goes
- [ ] Mute, then change the default mic to an unmuted one (Windows settings or a profile): the badge goes; change back to the muted one: it comes back
- [ ] Untick **Show an indicator…** while muted: the badge goes at once; tick it again: it comes back. The popup still shows either way
- [ ] Disable or unplug every microphone, press the shortcut: popup says "No microphone", nothing breaks
- [ ] Badge stays on top of normal windows and in a borderless fullscreen game
- [ ] Exit and restart Mixr while muted: the badge shows again on startup; the shortcut and the indicator setting are kept
- [ ] Cycle hotkey popup still shows the speaker icon

## 6. Daily use version

Setup: exit every Mixr. Open PowerShell in the repo folder. This section uses the stable copy, not F5.

- [ ] Run `.\publish-stable.ps1`: it builds without errors, ends with "Done", and the Mixr tray icon appears
- [ ] Task Manager > Details > Mixr.exe > right click > Open file location: it opens `C:\Apps\Mixr`
- [ ] `shell:startup` (Win+R) contains a `Mixr` shortcut pointing at `C:\Apps\Mixr\Mixr.exe`
- [ ] Sign out and back in: Mixr starts on its own, from `C:\Apps\Mixr`, with a single tray icon
- [ ] Change some settings (hide a device, set a shortcut, save a profile), exit Mixr from the tray, start `C:\Apps\Mixr\Mixr.exe` again: the settings are kept
- [ ] They are not in the F5 dev version (separate settings), and dev settings don't show up in the stable one
- [ ] With the stable one running, press F5 in Visual Studio: the dev one exits at once, the stable tray icon stays
- [ ] Exit the stable one, press F5, then start `C:\Apps\Mixr\Mixr.exe`: it exits at once, the dev one keeps running
- [ ] With the stable one running, make a small visible change (or just pull new commits) and run `.\publish-stable.ps1` again: it closes Mixr, copies, restarts it, and the change shows. Settings are kept
- [ ] Only one `Mixr` shortcut is in the Startup folder after running the script twice

## 7. Command line and mixr:// actions

Setup: run `.\publish-stable.ps1` once (it registers `mixr://` links). Have two or three playback devices, at least one profile saved, and the mic mute hotkey section working. Open PowerShell and `cd C:\Apps\Mixr`. Type the links into Win+R.

- [ ] `.\Mixr.exe cycle`: the default changes to the next "Include in cycle" device with the popup, and no second tray icon appears
- [ ] `.\Mixr.exe device "Speakers"` (use a real device name, or its custom name): it becomes the default, with the popup
- [ ] Part of a name works when only one device has it (e.g. `.\Mixr.exe device head`); a name nobody has shows "Not found: …"
- [ ] `.\Mixr.exe device "{0.0.0.00000000}.{…}"` with a real id (Settings > Devices doesn't show ids; copy one from the diagnostic data): it switches
- [ ] After `device`, `cycle` continues from that device
- [ ] `.\Mixr.exe profile "Work"`: the popup says "Profile: Work" and the profile applies; `profile work` (other case) works too; an unknown name shows "Not found: …"
- [ ] `.\Mixr.exe mic mute`, `mic unmute`, `mic toggle` (and plain `mic`): the mic state, popup and badge follow
- [ ] `.\Mixr.exe volume 30`: the default device goes to 30% and the popup says "<device>: 30%"
- [ ] `volume +10`, `volume -10`, `volume up`, `volume down 2`: change it by 10, 10, 5 and 2; `volume 150` gives 100, `volume -200` gives 0
- [ ] Win+R `mixr://cycle`, `mixr://device/Speakers`, `mixr://profile/Work`, `mixr://mic/toggle`, `mixr://volume/50`, `mixr://volume/up/10`: each does the same as the command line
- [ ] A name with spaces as a link: `mixr://profile/My%20Profile` and `mixr://device/My%20Headphones`
- [ ] The same links from a browser address bar (allow the "Open Mixr?" prompt): they work
- [ ] Unknown commands (`.\Mixr.exe nonsense`, `mixr://nonsense`) do nothing and nothing breaks
- [ ] Exit Mixr, then Win+R `mixr://volume/40`: Mixr starts, stays in the tray, and sets 40%
- [ ] Exit the stable one and press F5: `C:\Apps\Mixr\Mixr.exe cycle` and `mixr://cycle` control the dev one (no second icon)
- [ ] Stream Deck: a "System > Open" action with `C:\Apps\Mixr\Mixr.exe` and arguments `cycle`, or a "Website" action with `mixr://cycle`, works on a key press
- [ ] AutoHotkey: `^!F10::Run 'C:\Apps\Mixr\Mixr.exe volume up'` (v2) raises the volume with no flashing window
- [ ] Press a command key about 10 times very fast: no stutter, the final state matches the last popup
- [ ] Hotkeys, auto switch and the tray menu still work as before

## 8. Dev build look

Setup: Windows accent colour set to something that isn't red (e.g. the default blue). Have the stable copy installed (section 6).

- [ ] F5: the tray icon has a red dot in the bottom right corner; hover it: the tooltip starts with "Mixr Dev:"
- [ ] Open the flyout: slider fills and highlights are red, not blue. With Settings > Personalization > Colors > "Show accent colour on Start and taskbar" on, the flyout background is dark red
- [ ] Settings window: the title says "Settings - Mixr Dev", and the selected page and tick boxes are red
- [ ] Right click an app > move to device, and the full mixer window: selected items are red
- [ ] Switch Windows between light and dark mode: still red in both, and the dot stays
- [ ] Exit, start `C:\Apps\Mixr\Mixr.exe`: no dot, tooltip starts with "Mixr:", the title is "Settings", and the Windows accent colour is used
- [ ] Sign out and in: only the stable copy starts (the dev one is no longer under Settings > Apps > Startup)

## If something fails

Settings > About > open the diagnostic data and copy the lines starting with `DefaultDeviceCycler`, `DefaultDeviceSwitcher`, `AutoSwitchOnConnect`, `ProfileManager`, `MicMuteController`, `App RunCommand`, `CommandChannel`, `ProtocolRegistration` or `StorageFactory Using`.
