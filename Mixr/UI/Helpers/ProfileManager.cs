using Mixr.DataModel.Audio;
using Mixr.Diagnosis;
using Mixr.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace Mixr.UI.Helpers
{
    // Mixr: saves the current audio setup as a profile and applies saved profiles.
    public class ProfileManager
    {
        public event Action<AudioProfile> Applied;

        private readonly DeviceCollectionViewModel _playback;
        private readonly IAudioDeviceManager _recording;
        private readonly DefaultDeviceSwitcher _playbackSwitcher;
        private readonly DefaultDeviceSwitcher _recordingSwitcher;

        public ProfileManager(DeviceCollectionViewModel playback, IAudioDeviceManager recording, DefaultDeviceSwitcher playbackSwitcher, DefaultDeviceSwitcher recordingSwitcher)
        {
            _playback = playback;
            _recording = recording;
            _playbackSwitcher = playbackSwitcher;
            _recordingSwitcher = recordingSwitcher;
        }

        // Fills the profile with the current default output, default microphone and the routing of every running app.
        public void CaptureCurrent(AudioProfile profile)
        {
            var output = _playback.Default;
            profile.OutputDeviceId = output?.Id;
            profile.OutputDeviceName = output?.DisplayName;

            var input = _recording.Default;
            profile.InputDeviceId = input?.Id;
            profile.InputDeviceName = input?.DisplayName;

            profile.AppRoutes = _playback.AllDevices
                .SelectMany(d => d.Apps)
                .Where(a => a.IsMovable)
                .GroupBy(a => a.AppId)
                .Select(g => g.First())
                .Select(app =>
                {
                    var deviceId = ReadPersistedOutputDevice(app);
                    var device = _playback.AllDevices.FirstOrDefault(d => d.Id == deviceId);
                    return new AppRoute
                    {
                        AppId = app.AppId,
                        AppName = app.DisplayName,
                        DeviceId = string.IsNullOrEmpty(deviceId) ? null : deviceId,
                        DeviceName = device?.DisplayName,
                    };
                })
                .OrderBy(r => r.AppName)
                .ToArray();

            Trace.WriteLine($"ProfileManager CaptureCurrent {profile.Name}: output {profile.OutputDeviceId}, input {profile.InputDeviceId}, {profile.AppRoutes.Length} apps");
        }

        public void Apply(AudioProfile profile)
        {
            Trace.WriteLine($"ProfileManager Apply {profile.Name}");

            // Apps first, so apps set to follow the default move along with the default output below.
            foreach (var route in profile.AppRoutes ?? new AppRoute[] { })
            {
                ApplyAppRoute(route);
            }

            if (!string.IsNullOrEmpty(profile.OutputDeviceId))
            {
                if (_playback.AllDevices.Any(d => d.Id == profile.OutputDeviceId))
                {
                    _playbackSwitcher.RequestDefault(profile.OutputDeviceId, DefaultDeviceSwitcher.AllRoles);
                }
                else
                {
                    Trace.WriteLine($"ProfileManager Apply Output {profile.OutputDeviceId} is not connected");
                }
            }

            if (!string.IsNullOrEmpty(profile.InputDeviceId))
            {
                if (_recording.Devices.Any(d => d.Id == profile.InputDeviceId))
                {
                    _recordingSwitcher.RequestDefault(profile.InputDeviceId, DefaultDeviceSwitcher.AllRoles);
                }
                else
                {
                    Trace.WriteLine($"ProfileManager Apply Input {profile.InputDeviceId} is not connected");
                }
            }

            Applied?.Invoke(profile);
        }

        private void ApplyAppRoute(AppRoute route)
        {
            try
            {
                var app = _playback.AllDevices.SelectMany(d => d.Apps).FirstOrDefault(a => a.AppId == route.AppId && a.IsMovable);
                if (app == null)
                {
                    Trace.WriteLine($"ProfileManager ApplyAppRoute {route.AppName} is not running");
                    return;
                }

                DeviceViewModel device = null;
                if (!string.IsNullOrEmpty(route.DeviceId))
                {
                    device = _playback.AllDevices.FirstOrDefault(d => d.Id == route.DeviceId);
                    if (device == null)
                    {
                        Trace.WriteLine($"ProfileManager ApplyAppRoute {route.AppName} Device {route.DeviceId} is not connected");
                        return;
                    }
                }

                Trace.WriteLine($"ProfileManager ApplyAppRoute {route.AppName} -> {route.DeviceId ?? "default"}");
                _playback.MoveAppToDevice(app, device);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ProfileManager ApplyAppRoute {route.AppName} Failed: {ex}");
                ErrorReporter.LogWarning(ex);
            }
        }

        private static string ReadPersistedOutputDevice(IAppItemViewModel app)
        {
            try
            {
                return app.PersistedOutputDevice;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ProfileManager ReadPersistedOutputDevice {app.AppId} Failed: {ex}");
                return null;
            }
        }
    }
}
