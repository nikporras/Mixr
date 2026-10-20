using Mixr.DataModel.WindowsAudio;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mixr.DataModel.Audio
{
    public interface IAudioDeviceSession : IStreamWithVolumeControl
    {
        IEnumerable<IAudioDeviceSessionChannel> Channels { get; }
        IAudioDevice Parent { get; }
        string DisplayName { get; }
        string ExeName { get; }
        string IconPath { get; }
        bool IsDesktopApp { get; }
        bool IsSystemSoundsSession { get; }
        int ProcessId { get; }
        string AppId { get; }
        SessionState State { get; }
        ObservableCollection<IAudioDeviceSession> Children { get; }
    }
}