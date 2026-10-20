using Mixr.Interop.Helpers;
using Mixr.UI.Helpers;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Mixr.UI.ViewModels
{
    class MixrAboutPageViewModel : SettingsPageViewModel
    {
        public ICommand OpenDiagnosticsCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand OpenFeedbackCommand { get; }
        public ICommand OpenPrivacyPolicyCommand { get; }
        public string AboutText { get; }

        public bool IsTelemetryEnabled
        {
            get => _settings.IsTelemetryEnabled;
            set => _settings.IsTelemetryEnabled = value;
        }

        private readonly Action _openDiagnostics;
        private readonly AppSettings _settings;

        public MixrAboutPageViewModel(Action openDiagnostics, AppSettings settings) : base(null)
        {
            _settings = settings;
            _openDiagnostics = openDiagnostics;
            Glyph = "\xE946";
            Title = Properties.Resources.AboutTitle;
            AboutText = $"Mixr {App.PackageVersion}";

            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
            OpenFeedbackCommand = new RelayCommand(OpenGitHubIssueChooser);
            OpenPrivacyPolicyCommand = new RelayCommand(OpenPrivacyPolicy);
        }

        private void OpenDiagnostics()
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Trace.WriteLine($"MixrAboutPageViewModel OpenDiagnostics - CRASH");
                throw new Exception("This is an intentional crash.");
            }

            _openDiagnostics.Invoke();
        }

        private void OpenGitHubIssueChooser() => ProcessHelper.StartNoThrow("https://github.com/nikporras/Mixr/issues/new/choose");
        private void OpenAbout() => ProcessHelper.StartNoThrow("https://github.com/nikporras/Mixr");
        private void OpenPrivacyPolicy() => ProcessHelper.StartNoThrow("https://github.com/nikporras/Mixr/blob/master/PRIVACY.md");
    }
}
