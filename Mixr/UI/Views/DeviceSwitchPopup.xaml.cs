using Mixr.Interop;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Mixr.UI.Views
{
    // Mixr: briefly shows the new default device's name after the cycle hotkey is pressed.
    public partial class DeviceSwitchPopup : Window
    {
        private static readonly TimeSpan s_displayTime = TimeSpan.FromSeconds(1.5);
        private const double BottomMargin = 48;

        private readonly DispatcherTimer _hideTimer;

        public DeviceSwitchPopup()
        {
            InitializeComponent();

            _hideTimer = new DispatcherTimer { Interval = s_displayTime };
            _hideTimer.Tick += (_, __) =>
            {
                _hideTimer.Stop();
                Hide();
            };

            // Never take focus or mouse input away from whatever the user is doing.
            SourceInitialized += (_, __) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var exStyle = User32.GetWindowLong(hwnd, User32.GWL.GWL_EXSTYLE);
                User32.SetWindowLong(hwnd, User32.GWL.GWL_EXSTYLE, exStyle | User32.WS_EX_NOACTIVATE | User32.WS_EX_TOOLWINDOW | User32.WS_EX_TRANSPARENT);
            };
            SizeChanged += (_, __) => Reposition();
        }

        public void ShowDeviceName(string name)
        {
            DeviceNameText.Text = name ?? "";
            if (!IsVisible)
            {
                Show();
            }
            Reposition();

            _hideTimer.Stop();
            _hideTimer.Start();
        }

        private void Reposition()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left + (workArea.Width - ActualWidth) / 2;
            Top = workArea.Bottom - ActualHeight - BottomMargin;
        }
    }
}
