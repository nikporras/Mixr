using System;
using Mixr.UI.ViewModels;
using System.Collections.Generic;

namespace Mixr.Extensibility
{
    public interface IMixrAddonAppContent
    {
        object GetContentForApp(string deviceId, string appId, Action requestClose);
        IEnumerable<ContextMenuItem> GetContextMenuItemsForApp(string deviceId, string appId);
    }
}
