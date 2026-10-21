using Mixr.UI.ViewModels;
using System.Diagnostics;
using System.Linq;

namespace Mixr.UI.Helpers
{
    // Mixr: steps the default output through the devices ticked "Include in cycle".
    // The cycle keeps its own position, because Windows reports the new default only after the switch
    // completes, so rapid presses would otherwise keep picking the same "next" device.
    public class DefaultDeviceCycler
    {
        private readonly DeviceCollectionViewModel _devices;
        private readonly AppSettings _settings;
        private readonly DefaultDeviceSwitcher _switcher;
        private string _positionId;

        public DefaultDeviceCycler(DeviceCollectionViewModel devices, AppSettings settings, DefaultDeviceSwitcher switcher)
        {
            _devices = devices;
            _settings = settings;
            _switcher = switcher;
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
            _switcher.RequestDefault(next.Id);
            return next;
        }

        private void OnDefaultChanged(object sender, DeviceViewModel newDefault)
        {
            // The default was changed some other way (tray menu, Windows settings): continue from there next time.
            if (!_switcher.IsApplying && newDefault?.Id != _positionId)
            {
                _positionId = null;
            }
        }
    }
}
