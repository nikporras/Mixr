using Mixr.Diagnosis;
using Mixr.Interop.MMDeviceAPI;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mixr.UI.Helpers
{
    // Mixr: sets the default output off the UI thread. Windows can take a while to switch, so requests that
    // arrive while one is in flight collapse into the latest target. Windows reports the result through the
    // usual default-changed notification.
    public class DefaultDeviceSwitcher
    {
        // The cycle hotkey and auto switch set eMultimedia only, as the rest of Mixr does.
        public static readonly ERole[] DefaultRoles = { ERole.eMultimedia };

        // Profiles set "Default device" and "Default communications device" together.
        public static readonly ERole[] AllRoles = { ERole.eConsole, ERole.eMultimedia, ERole.eCommunications };

        private readonly object _lock = new object();

        // Guarded by _lock.
        private string _pendingId;
        private ERole[] _pendingRoles;
        private bool _isApplying;

        public bool IsApplying
        {
            get
            {
                lock (_lock)
                {
                    return _isApplying;
                }
            }
        }

        public void RequestDefault(string deviceId) => RequestDefault(deviceId, DefaultRoles);

        public void RequestDefault(string deviceId, ERole[] roles)
        {
            lock (_lock)
            {
                _pendingId = deviceId;
                _pendingRoles = roles;
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
                ERole[] roles;
                lock (_lock)
                {
                    deviceId = _pendingId;
                    roles = _pendingRoles;
                    _pendingId = null;
                    _pendingRoles = null;
                    if (deviceId == null)
                    {
                        _isApplying = false;
                        return;
                    }
                }

                try
                {
                    Trace.WriteLine($"DefaultDeviceSwitcher ApplyPendingDefault {deviceId} ({string.Join(",", roles)})");
                    if (policyConfig == null)
                    {
                        policyConfig = new AutoPolicyConfigClientWin7();
                    }
                    foreach (var role in roles)
                    {
                        policyConfig.SetDefaultEndpoint(deviceId, role);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"DefaultDeviceSwitcher ApplyPendingDefault {deviceId} Failed: {ex}");
                    ErrorReporter.LogWarning(ex);
                }
            }
        }
    }
}
