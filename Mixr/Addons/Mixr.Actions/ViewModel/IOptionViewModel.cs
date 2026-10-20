using System.Collections.ObjectModel;

namespace Mixr.Actions.ViewModel
{
    interface IOptionViewModel
    {
        ObservableCollection<Option> All { get; }
        Option Selected { get; set; }
    }
}