using System;
using System.Collections.ObjectModel;

namespace Mixr.DataModel.Audio
{
    public interface IAudioDevice : IStreamWithVolumeControl
    {
        string DisplayName { get; }
        string IconPath { get; }
        IAudioDeviceManager Parent { get; }
        ObservableCollection<IAudioDeviceSession> Groups { get; }
        void AddFilter(Func<ObservableCollection<IAudioDeviceSession>, ObservableCollection<IAudioDeviceSession>> filter);
    }
}