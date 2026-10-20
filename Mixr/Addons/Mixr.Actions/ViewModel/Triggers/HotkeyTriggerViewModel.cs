using Mixr.Actions.DataModel.Serialization;

namespace Mixr.Actions.ViewModel.Triggers
{
    class HotkeyTriggerViewModel : PartViewModel
    {
        public HotkeyViewModel Hotkey { get; }

        private HotkeyTrigger _trigger;

        public HotkeyTriggerViewModel(HotkeyTrigger trigger) : base(trigger)
        {
            _trigger = trigger;

            Hotkey = new HotkeyViewModel(trigger);
            Attach(Hotkey);
        }
    }
}
