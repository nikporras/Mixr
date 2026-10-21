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
        private readonly object _lock = new object();

        // Guarded by _lock.
        private string _pendingId;
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

        public void RequestDefault(string deviceId)
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
                    Trace.WriteLine($"DefaultDeviceSwitcher ApplyPendingDefault {deviceId}");
                    if (policyConfig == null)
                    {
                        policyConfig = new AutoPolicyConfigClientWin7();
                    }
                    policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
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
