using System.ComponentModel;

namespace Mixr.DataModel.WindowsAudio
{
    public interface IAudioDeviceChannel : INotifyPropertyChanged
    {
        float Level { get; set; }
    }
}