using Mixr.Extensions;
using Mixr.UI.Helpers;
using Mixr.UI.ViewModels;
using Mixr.Actions.DataModel.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mixr.Actions.ViewModel
{
    public class ActionsCategoryViewModel : SettingsCategoryViewModel
    {
        public ActionsCategoryViewModel()
            : base(Properties.Resources.MyActionsText, "\xE950", Properties.Resources.AddonDescriptionText, MixrActionsAddon.Current.Manifest.Id, new ObservableCollection<SettingsPageViewModel>())
        {
            // Get a 'fresh' copy so that we can edit the objects and still go back later.
            var actions = MixrActionsAddon.Current.Actions;
            MixrActionsAddon.Current.Actions = MixrActionsAddon.Current.Actions;

            Pages.AddRange(actions.Select(a => new MixrActionViewModel(this, a)));
            Pages.Add(new ImportExportPageViewModel(this));

            Toolbar = new ToolbarItemViewModel[] { new ToolbarItemViewModel{
                Command = new RelayCommand(() =>
                {
                    var vm = new MixrActionViewModel(this, new MixrAction { DisplayName = Properties.Resources.NewActionText });
                    vm.IsWorkSaved = false;
                    vm.IsPersisted = false;

                    vm.PropertyChanged += (_, e) =>
                    {
                        if (e.PropertyName == nameof(vm.IsSelected) &&
                            vm.IsSelected && !Pages.Contains(vm))
                        {
                            Pages.Insert(0, vm);
                        }
                    };

                    Selected = vm;
                }),
                DisplayName = Properties.Resources.NewActionText,
                Glyph = "\xE948",
                GlyphFontSize = 15,
            } };

            if (Pages.Count == 2)
            {
                Toolbar[0].Command.Execute(null);
            }
        }

        internal void ReloadSavedPages()
        {
            foreach (var item in Pages.Where(p => p is MixrActionViewModel).ToList())
            {
                Pages.Remove(item);
            }

            Pages.InsertRange(0, new System.Collections.ObjectModel.ObservableCollection<SettingsPageViewModel>(MixrActionsAddon.Current.Actions.Select(a => new MixrActionViewModel(this, a))));
            Selected = Pages[0];
        }

        public void Delete(MixrActionViewModel mixrActionViewModel, bool promptOverride = false)
        {
            Action doRemove = () =>
            {
                var actions = MixrActionsAddon.Current.Actions.ToList();
                if (actions.Any(a => a.Id == mixrActionViewModel.Id))
                {
                    actions.Remove(item => item.Id == mixrActionViewModel.Id);
                }
                MixrActionsAddon.Current.Actions = actions.ToArray();

                if (Pages.Any(a => a == mixrActionViewModel))
                {
                    Pages.Remove(mixrActionViewModel);
                }
            };

            if (mixrActionViewModel.IsPersisted && !promptOverride)
            {
                _parent.ShowDialog(Properties.Resources.DeleteActionDialogTitle, Properties.Resources.DeleteActionDialogText,
                    Properties.Resources.DeleteActionDialogYesText, Properties.Resources.DeleteActionDialogNoText, doRemove, () => { });
            }
            else
            {
                doRemove();
            }
        }

        public void Save(MixrActionViewModel mixrActionViewModel)
        {
            var actions = MixrActionsAddon.Current.Actions.ToList();
            if (actions.Any(a => a.Id == mixrActionViewModel.Id))
            {
                actions.Remove(item => item.Id == mixrActionViewModel.Id);
            }
            actions.Insert(0, mixrActionViewModel.GetAction());
            MixrActionsAddon.Current.Actions = actions.ToArray();
            mixrActionViewModel.IsWorkSaved = true;

            if (Pages.Any(a => a == mixrActionViewModel))
            {
                Pages.Remove(mixrActionViewModel);
            }
            Pages.Insert(0, mixrActionViewModel);
            Selected = Pages[0];
        }

        public void CompleteNavigation(NavigationCookie cookie)
        {
            _parent.CompleteNavigation(cookie);
        }
    }
}
