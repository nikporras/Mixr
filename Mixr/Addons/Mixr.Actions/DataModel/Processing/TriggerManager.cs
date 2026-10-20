using Mixr.Extensibility;
using Mixr.Interop.Helpers;
using Mixr.Actions.DataModel.Enum;
using Mixr.Actions.DataModel.Serialization;
using System;
using System.Collections.Generic;

namespace Mixr.Actions.DataModel.Processing
{
    class TriggerManager
    {
        public event Action<BaseTrigger> Triggered;

        private List<EventTrigger> _eventTriggers = new List<EventTrigger>();
        private AudioTriggerManager _audioManager;

        public TriggerManager()
        {
            _audioManager = new AudioTriggerManager();
            _audioManager.Triggered += (t) => Triggered?.Invoke(t);
        }

        public void Clear()
        {
            ProcessWatcher.Current.Clear();
            _eventTriggers.Clear();
            _audioManager.Clear();
        }

        public void OnEvent(AddonEventKind evt)
        {
            foreach (var trigger in _eventTriggers)
            {
                if ((trigger.Option == MixrEventKind.Startup && evt == AddonEventKind.InitializeAddon) ||
                    (trigger.Option == MixrEventKind.Shutdown && evt == AddonEventKind.AppShuttingDown))
                {
                    Triggered?.Invoke(trigger);
                }
            }
        }

        public void Register(BaseTrigger trig)
        {
            if (trig is ProcessTrigger)
            {
                var trigger = (ProcessTrigger)trig;
                if (!string.IsNullOrWhiteSpace(trigger.Text))
                {
                    if (trigger.Option == ProcessEventKind.Start)
                    {
                        ProcessWatcher.Current.RegisterStart(trigger.Text, () => Triggered?.Invoke(trig));
                    }
                    else
                    {
                        ProcessWatcher.Current.RegisterStop(trigger.Text, () => Triggered?.Invoke(trig));
                    }
                }
            }
            else if (trig is EventTrigger)
            {
                _eventTriggers.Add((EventTrigger)trig);
            }
            else if (trig is DeviceEventTrigger)
            {
                _audioManager.Register(trig);
            }
            else if (trig is AppEventTrigger)
            {
                _audioManager.Register(trig);
            }
            else if (trig is HotkeyTrigger)
            {
                var trigger = (HotkeyTrigger)trig;

                HotkeyManager.Current.Register(trigger.Option);
                HotkeyManager.Current.KeyPressed += (data) =>
                {
                    if (data.Equals(trigger.Option))
                    {
                        Triggered?.Invoke(trig);
                    }
                };
            }
            else if (trig is ContextMenuTrigger)
            {
                // Nothing to do.
            }
            else throw new NotImplementedException();
        }
    }
}
