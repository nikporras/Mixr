using Mixr.Diagnosis;
using Mixr.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace Mixr.UI.Helpers
{
    // Mixr: while a profile is active, moves an app from its routes to the saved device when the app starts.
    // An app "starts" when it shows up while it wasn't running anywhere a moment ago, so moving a running app
    // (which briefly shows it on both devices, or removes and adds it) isn't undone.
    public class ActiveProfileRouter
    {
        private static readonly TimeSpan RecentlyRunning = TimeSpan.FromSeconds(5);

        private readonly DeviceCollectionViewModel _playback;
        private readonly AppSettings _settings;
        private readonly ProfileManager _profileManager;
        private readonly HashSet<DeviceViewModel> _watched = new HashSet<DeviceViewModel>();
        private readonly Dictionary<string, DateTime> _lastGone = new Dictionary<string, DateTime>();

        public ActiveProfileRouter(DeviceCollectionViewModel playback, AppSettings settings, ProfileManager profileManager)
        {
            _playback = playback;
            _settings = settings;
            _profileManager = profileManager;

            _playback.AllDevices.CollectionChanged += (_, __) => WatchDevices();
            WatchDevices();
        }

        private void WatchDevices()
        {
            // Apps already on a device when it's connected (or when Mixr starts) aren't starting, so only new ones count.
            foreach (var device in _playback.AllDevices.Where(d => !_watched.Contains(d)).ToList())
            {
                _watched.Add(device);
                device.Apps.CollectionChanged += OnAppsChanged;
            }

            foreach (var device in _watched.Where(d => !_playback.AllDevices.Contains(d)).ToList())
            {
                _watched.Remove(device);
                device.Apps.CollectionChanged -= OnAppsChanged;
            }
        }

        private void OnAppsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.OldItems != null)
                {
                    foreach (IAppItemViewModel app in e.OldItems)
                    {
                        if (!IsRunningAnywhere(app.AppId, app))
                        {
                            _lastGone[app.AppId] = DateTime.UtcNow;
                        }
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (IAppItemViewModel app in e.NewItems)
                    {
                        if (app.IsMovable && !IsRunningAnywhere(app.AppId, app) && !WasRecentlyRunning(app.AppId))
                        {
                            OnAppStarted(app);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ActiveProfileRouter OnAppsChanged Failed: {ex}");
                ErrorReporter.LogWarning(ex);
            }
        }

        private void OnAppStarted(IAppItemViewModel app)
        {
            var profile = _settings.ActiveProfile;
            var route = profile?.AppRoutes?.FirstOrDefault(r => r.AppId == app.AppId);
            if (route == null)
            {
                return;
            }

            var wanted = string.IsNullOrEmpty(route.DeviceId) ? null : route.DeviceId;
            var persisted = ProfileManager.ReadPersistedOutputDevice(app);
            if (string.IsNullOrEmpty(persisted))
            {
                persisted = null;
            }

            if (wanted == persisted)
            {
                Trace.WriteLine($"ActiveProfileRouter OnAppStarted {route.AppName} already on {wanted ?? "default"}");
                return;
            }

            Trace.WriteLine($"ActiveProfileRouter OnAppStarted {route.AppName} ({profile.Name}) {persisted ?? "default"} -> {wanted ?? "default"}");
            // Not while the Apps collection is still raising its change.
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => _profileManager.ApplyAppRoute(route)));
        }

        // Excluding this item itself, since it is already in its device's Apps when added and gone when removed.
        private bool IsRunningAnywhere(string appId, IAppItemViewModel except) =>
            _playback.AllDevices.SelectMany(d => d.Apps).Any(a => a != except && a.AppId == appId);

        private bool WasRecentlyRunning(string appId) =>
            _lastGone.TryGetValue(appId, out var goneAt) && DateTime.UtcNow - goneAt < RecentlyRunning;
    }
}
