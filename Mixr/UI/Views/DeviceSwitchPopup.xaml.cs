using Mixr.Extensions;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Mixr.UI.Views
{
    // Mixr: briefly shows what just changed (new default device, applied profile, mic mute) at the bottom of the screen.
    public partial class DeviceSwitchPopup : Window
    {
        public const string SpeakerGlyph = "\xE7F5";
        public const string MicrophoneGlyph = "\xE720";
        public const string MicrophoneOffGlyph = "\xEC54";

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

            SourceInitialized += (_, __) => this.MakeClickThroughOverlay();
            SizeChanged += (_, __) => Reposition();
        }

        public void ShowText(string text, string glyph = SpeakerGlyph)
        {
            GlyphText.Text = glyph;
            DeviceNameText.Text = text ?? "";
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
