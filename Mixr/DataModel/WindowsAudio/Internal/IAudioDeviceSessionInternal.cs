using Mixr.DataModel.Audio;
using System;

namespace Mixr.DataModel.WindowsAudio.Internal
{
    interface IAudioDeviceSessionInternal : IAudioDeviceSession
    {
        Guid GroupingParam { get; }
        void Hide();
        void UnHide();
        void MoveToDevice(string id, bool hide);
        void UpdatePeakValueBackground();
    }
}
