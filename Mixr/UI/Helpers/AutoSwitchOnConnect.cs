using Mixr.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace Mixr.UI.Helpers
{
    // Mixr: makes a device ticked "Switch to it when it connects" the default output when it appears, and
    // switches back to the previous default when it disconnects, unless the user picked another device meanwhile.
    public class AutoSwitchOnConnect
    {
        // Windows picks a new default by itself when the default device goes away. A default change this
        // close to the disconnect is that, not the user choosing another device.
        private static readonly TimeSpan s_disconnectGrace = TimeSpan.FromSeconds(2);

        private class SwitchRecord
        {
            public string DeviceId;
            public string PreviousId;
            public DateTime? OverriddenAt;
        }

        public event Action<DeviceViewModel> Switched;

        private readonly DeviceCollectionViewModel _devices;
        private readonly AppSettings _settings;
        private readonly DefaultDeviceSwitcher _switcher;
        private readonly List<SwitchRecord> _records = new List<SwitchRecord>();

        public AutoSwitchOnConnect(DeviceCollectionViewModel devices, AppSettings settings, DefaultDeviceSwitcher switcher)
        {
            _devices = devices;
            _settings = settings;
            _switcher = switcher;
            _devices.AllDevices.CollectionChanged += OnDevicesChanged;
            _devices.DefaultChanged += OnDefaultChanged;
        }

        private void OnDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (DeviceViewModel device in e.NewItems)
                    {
                        OnConnected(device);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (DeviceViewModel device in e.OldItems)
                    {
                        OnDisconnected(device);
                    }
                    break;
            }
        }

        private void OnConnected(DeviceViewModel device)
        {
            if (!_settings.IsDeviceAutoSwitch(device.Id))
            {
                return;
            }

            _records.RemoveAll(r => r.DeviceId == device.Id);

            var previousId = _devices.Default?.Id;
            if (previousId == device.Id)
            {
                return;
            }

            Trace.WriteLine($"AutoSwitchOnConnect OnConnected {device.Id} (previous {previousId})");
            _records.Add(new SwitchRecord { DeviceId = device.Id, PreviousId = previousId });
            _switcher.RequestDefault(device.Id);
            Switched?.Invoke(device);
        }

        private void OnDisconnected(DeviceViewModel device)
        {
            var record = _records.FirstOrDefault(r => r.DeviceId == device.Id);
            if (record == null)
            {
                return;
            }
            _records.Remove(record);

            if (record.OverriddenAt.HasValue && DateTime.Now - record.OverriddenAt.Value > s_disconnectGrace)
            {
                Trace.WriteLine($"AutoSwitchOnConnect OnDisconnected {device.Id} Default was changed by the user, not switching back");
                return;
            }

            var previous = _devices.AllDevices.FirstOrDefault(d => d.Id == record.PreviousId);
            if (previous == null)
            {
                Trace.WriteLine($"AutoSwitchOnConnect OnDisconnected {device.Id} Previous default {record.PreviousId} is gone, not switching back");
                return;
            }

            Trace.WriteLine($"AutoSwitchOnConnect OnDisconnected {device.Id} Switching back to {previous.Id}");
            _switcher.RequestDefault(previous.Id);
            Switched?.Invoke(previous);
        }

        private void OnDefaultChanged(object sender, DeviceViewModel newDefault)
        {
            if (_switcher.IsApplying || newDefault == null)
            {
                return;
            }

            // Becoming default is what an auto-switched device should be; anything else is the user (or Windows,
            // on disconnect, which s_disconnectGrace allows for) choosing another device.
            var record = _records.FirstOrDefault(r => r.DeviceId == newDefault.Id);
            if (record != null)
            {
                record.OverriddenAt = null;
                return;
            }

            foreach (var r in _records.Where(r => !r.OverriddenAt.HasValue))
            {
                r.OverriddenAt = DateTime.Now;
            }
        }
    }
}
