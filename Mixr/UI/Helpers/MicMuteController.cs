using Mixr.DataModel.Audio;
using Mixr.Diagnosis;
using Mixr.UI.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Mixr.UI.Helpers
{
    // Mixr: mutes and unmutes the default microphone, and shows MicMuteIndicator while it is muted,
    // whoever muted it (the hotkey, Windows settings, a headset button).
    public class MicMuteController
    {
        private readonly IAudioDeviceManager _recording;
        private readonly AppSettings _settings;
        private IAudioDevice _mic;
        private MicMuteIndicator _indicator;

        public MicMuteController(IAudioDeviceManager recording, AppSettings settings)
        {
            _recording = recording;
            _settings = settings;
            _recording.DefaultChanged += (_, device) => Attach(device);
            _settings.ShowMicMuteIndicatorChanged += UpdateIndicator;
            Attach(_recording.Default);
        }

        // Returns the new mute state, or null if there is no microphone.
        public bool? ToggleMute() => SetMute(null);

        // Mutes (true), unmutes (false) or toggles (null) the default microphone.
        // Returns the new mute state, or null if there is no microphone.
        public bool? SetMute(bool? mute)
        {
            var mic = _recording.Default;
            if (mic == null)
            {
                Trace.WriteLine("MicMuteController SetMute No microphone");
                return null;
            }

            // Windows confirms through the volume callback, which updates the indicator.
            var isMuted = mute ?? !mic.IsMuted;
            mic.IsMuted = isMuted;
            Trace.WriteLine($"MicMuteController SetMute {mic.Id} IsMuted={isMuted}");
            return isMuted;
        }

        private void Attach(IAudioDevice mic)
        {
            if (_mic != null)
            {
                _mic.PropertyChanged -= OnMicPropertyChanged;
            }

            _mic = mic;
            Trace.WriteLine($"MicMuteController Attach {_mic?.Id}");

            if (_mic != null)
            {
                _mic.PropertyChanged += OnMicPropertyChanged;
            }
            UpdateIndicator();
        }

        private void OnMicPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAudioDevice.IsMuted))
            {
                UpdateIndicator();
            }
        }

        private void UpdateIndicator()
        {
            try
            {
                var isVisible = _settings.ShowMicMuteIndicator && _mic != null && _mic.IsMuted;
                if (_indicator == null)
                {
                    if (!isVisible)
                    {
                        return;
                    }
                    _indicator = new MicMuteIndicator();
                }
                _indicator.SetVisible(isVisible);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"MicMuteController UpdateIndicator Failed: {ex}");
                ErrorReporter.LogWarning(ex);
            }
        }
    }
}
