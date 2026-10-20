using System.ComponentModel;

namespace Mixr.DataModel.WindowsAudio
{
    public interface IAudioDeviceSessionChannel : INotifyPropertyChanged
    {
        float Level { get; set; }
    }
}