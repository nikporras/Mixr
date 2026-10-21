using Mixr.Extensibility.Hosting;
using Mixr.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mixr.UI.ViewModels
{
    class FocusedDeviceViewModel : IFocusedViewModel
    {
        public event Action RequestClose;

        public string DisplayName { get; }
        public ObservableCollection<ToolbarItemViewModel> Toolbar { get; }

        public ObservableCollection<object> Addons { get; }

        // Mixr: always applicable, since every device gets the Hide / Show button.
        public bool IsApplicable => true;

        public FocusedDeviceViewModel(DeviceCollectionViewModel mainViewModel, DeviceViewModel device)
        {
            DisplayName = device.DisplayName;
            Toolbar = new ObservableCollection<ToolbarItemViewModel>();
            Toolbar.Add(new ToolbarItemViewModel
            {
                GlyphFontSize = 10,
                DisplayName = Properties.Resources.CloseButtonAccessibleText,
                Glyph = "\uE8BB",
                Command = new RelayCommand(() => RequestClose.Invoke())
            });

            var settings = mainViewModel.Settings;
            if (settings != null)
            {
                var isHidden = device.IsHidden;
                Toolbar.Insert(0, new ToolbarItemViewModel
                {
                    GlyphFontSize = 16,
                    DisplayName = isHidden ? Properties.Resources.DeviceShowInMixrText : Properties.Resources.HideDeviceText,
                    Glyph = isHidden ? "\uE890" : "\uED1A",
                    Command = new RelayCommand(() =>
                    {
                        RequestClose.Invoke();
                        settings.SetDeviceHidden(device.Id, !isHidden);
                    })
                });
            }

            var contentItems = AddonManager.Host.DeviceContentItems;
            if (contentItems != null)
            {
                Addons = new ObservableCollection<object>(contentItems.Select(a => a.GetContentForDevice(device.Id, () => RequestClose.Invoke())).Where(a => a != null).ToArray());

                var menuItems = contentItems.SelectMany(a => a.GetContextMenuItemsForDevice(device.Id)).Where(m => m != null);
                if (menuItems.Any())
                {
                    Toolbar.Insert(0, new ToolbarItemViewModel
                    {
                        GlyphFontSize = 16,
                        DisplayName = Properties.Resources.MoreCommandsAccessibleText,
                        Glyph = "\uE10C",
                        Menu = new ObservableCollection<ContextMenuItem>(menuItems)
                    });
                }
            }
        }

        public void Closing() { }
    }
}
