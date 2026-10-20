using Mixr.UI.ViewModels;
using System.Collections.Generic;

namespace Mixr.Extensibility
{
    public interface IMixrAddonNotificationAreaContextMenu
    {
        IEnumerable<ContextMenuItem> NotificationAreaContextMenuItems { get; }
    }
}