using Mixr.Diagnosis;
using Mixr.Interop.MMDeviceAPI;
using Mixr.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Mixr.UI.Helpers
{
    // Mixr: steps the default output through the devices ticked "Include in cycle".
    // The cycle keeps its own position, because Windows reports the new default only after the switch
    // completes, so rapid presses would otherwise keep picking the same "next" device. The switch itself
    // runs off the UI thread, and presses that arrive while one is in flight collapse into the latest target.
    public class DefaultDeviceCycler
    {
        private readonly DeviceCollectionViewModel _devices;
        private readonly AppSettings _settings;
        private readonly object _lock = new object();

        // UI thread only.
        private string _positionId;

        // Guarded by _lock.
        private string _pendingId;
        private bool _isApplying;

        public DefaultDeviceCycler(DeviceCollectionViewModel devices, AppSettings settings)
        {
            _devices = devices;
            _settings = settings;
            _devices.DefaultChanged += OnDefaultChanged;
        }

        // Advances the cycle and returns the device that will become the default, or null if no devices are ticked.
        public DeviceViewModel Next()
        {
            var cycle = _devices.AllDevices.Where(d => _settings.IsDeviceInCycle(d.Id)).ToList();
            if (!cycle.Any())
            {
                Trace.WriteLine("DefaultDeviceCycler Next No devices in cycle");
                return null;
            }

            var currentId = cycle.Any(d => d.Id == _positionId) ? _positionId : _devices.Default?.Id;
            var currentIndex = cycle.FindIndex(d => d.Id == currentId);
            var next = cycle[(currentIndex + 1) % cycle.Count];

            Trace.WriteLine($"DefaultDeviceCycler Next {next.Id}");
            _positionId = next.Id;
            RequestDefault(next.Id);
            return next;
        }

        private void RequestDefault(string deviceId)
        {
            lock (_lock)
            {
                _pendingId = deviceId;
                if (_isApplying)
                {
                    return;
                }
                _isApplying = true;
            }

            Task.Run(ApplyPendingDefault);
        }

        private void ApplyPendingDefault()
        {
            AutoPolicyConfigClientWin7 policyConfig = null;
            while (true)
            {
                string deviceId;
                lock (_lock)
                {
                    deviceId = _pendingId;
                    _pendingId = null;
                    if (deviceId == null)
                    {
                        _isApplying = false;
                        return;
                    }
                }

                try
                {
                    Trace.WriteLine($"DefaultDeviceCycler ApplyPendingDefault {deviceId}");
                    if (policyConfig == null)
                    {
                        policyConfig = new AutoPolicyConfigClientWin7();
                    }
                    policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"DefaultDeviceCycler ApplyPendingDefault {deviceId} Failed: {ex}");
                    ErrorReporter.LogWarning(ex);
                }
            }
        }

        private void OnDefaultChanged(object sender, DeviceViewModel newDefault)
        {
            bool isApplying;
            lock (_lock)
            {
                isApplying = _isApplying;
            }

            // The default was changed some other way (tray menu, Windows settings): continue from there next time.
            if (!isApplying && newDefault?.Id != _positionId)
            {
                _positionId = null;
            }
        }
    }
}
