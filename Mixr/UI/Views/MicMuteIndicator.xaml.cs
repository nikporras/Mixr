using Mixr.Extensions;
using System.Windows;

namespace Mixr.UI.Views
{
    // Mixr: stays in the top right corner of the screen while the default microphone is muted.
    public partial class MicMuteIndicator : Window
    {
        private const double EdgeMargin = 16;

        public MicMuteIndicator()
        {
            InitializeComponent();

            SourceInitialized += (_, __) => this.MakeClickThroughOverlay();
            SizeChanged += (_, __) => Reposition();
        }

        public void SetVisible(bool isVisible)
        {
            if (isVisible && !IsVisible)
            {
                Show();
                Reposition();
            }
            else if (!isVisible && IsVisible)
            {
                Hide();
            }
        }

        private void Reposition()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - ActualWidth - EdgeMargin;
            Top = workArea.Top + EdgeMargin;
        }
    }
}
