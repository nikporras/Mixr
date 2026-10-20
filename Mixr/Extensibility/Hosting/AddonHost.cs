using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Mixr.Extensibility.Hosting
{
    public class AddonHost
    {
        [ImportMany(typeof(MixrAddon))]
        public List<MixrAddon> Addons { get; set; }

        // Optional cast to any of the below:
        public List<IMixrAddonEvents> Events { get; set; }
        public List<IMixrAddonNotificationAreaContextMenu> TrayContextMenuItems { get; set; }
        public List<IMixrAddonAppContent> AppContentItems { get; set; }
        public List<IMixrAddonDeviceContent> DeviceContentItems { get; set; }
        public List<IMixrAddonSettingsPage> SettingsItems { get; set; }
    }
}
