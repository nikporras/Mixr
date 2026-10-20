using System;
using Mixr.UI.ViewModels;
using System.Collections.Generic;

namespace Mixr.Extensibility
{
    public interface IMixrAddonDeviceContent
    {
        object GetContentForDevice(string deviceId, Action requestClose);
        IEnumerable<ContextMenuItem> GetContextMenuItemsForDevice(string deviceId);
    }
}
