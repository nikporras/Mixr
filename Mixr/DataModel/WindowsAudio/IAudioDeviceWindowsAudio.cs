using Mixr.DataModel.Audio;
using System.Collections.Generic;

namespace Mixr.DataModel.WindowsAudio
{
    public interface IAudioDeviceWindowsAudio : IAudioDevice
    {
        IEnumerable<IAudioDeviceChannel> Channels { get; }
        string EnumeratorName { get; }
        string InterfaceName { get; }
        string DeviceDescription { get; }
    }
}
