using System.Windows;

namespace Mixr.UI.ViewModels
{
    public interface IPopupHostViewModel
    {
        void OpenPopup(object vm, FrameworkElement container);
    }
}