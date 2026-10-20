using System;

namespace Mixr.UI.Helpers
{
    public interface IShellNotifyIconSource
    {
        event Action<IShellNotifyIconSource> Changed;
        System.Drawing.Icon Current { get; }
        void OnMouseOverChanged(bool isMouseOver);
        void CheckForUpdate();
    }
}
