using Mixr.UI.ViewModels;
using System.ComponentModel;

namespace Mixr.Actions.ViewModel
{
    public class MixrActionPageHeaderViewModel : SettingsPageHeaderViewModel
    {
        MixrActionViewModel _parent;

        public ToolbarItemViewModel[] Toolbar => _parent.Toolbar;
        public string DisplayName { get => _parent.DisplayName; set => _parent.DisplayName = value; }
        public bool IsEditClicked { get => _parent.IsEditClicked; set => _parent.IsEditClicked = value; }
        public bool IsWorkSaved => _parent.IsWorkSaved;
        public MixrActionPageHeaderViewModel(MixrActionViewModel parent) : base(parent)
        {
            _parent = parent;
            ((INotifyPropertyChanged)_parent).PropertyChanged += MixrActionPageHeaderViewModel_PropertyChanged;
        }

        private void MixrActionPageHeaderViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }
    }
}