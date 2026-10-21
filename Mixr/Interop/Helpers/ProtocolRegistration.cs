using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace Mixr.Interop.Helpers
{
    // Mixr: registers mixr:// links for the current user, pointing at this Mixr.exe. Only the unpackaged
    // (stable) Mixr does this; the package doesn't declare the protocol, so Windows never has to ask which
    // one opens a link. Whichever Mixr is running gets the command either way.
    static class ProtocolRegistration
    {
        public static void RegisterForCurrentUser(string scheme)
        {
            try
            {
                var exe = Process.GetCurrentProcess().MainModule.FileName;
                var command = $"\"{exe}\" \"%1\"";
                var root = $@"Software\Classes\{scheme}";

                using (var key = Registry.CurrentUser.OpenSubKey($@"{root}\shell\open\command"))
                {
                    if (key?.GetValue("") as string == command)
                    {
                        return;
                    }
                }

                using (var key = Registry.CurrentUser.CreateSubKey(root))
                {
                    key.SetValue("", "URL:Mixr");
                    key.SetValue("URL Protocol", "");
                }
                using (var key = Registry.CurrentUser.CreateSubKey($@"{root}\DefaultIcon"))
                {
                    key.SetValue("", $"\"{exe}\",0");
                }
                using (var key = Registry.CurrentUser.CreateSubKey($@"{root}\shell\open\command"))
                {
                    key.SetValue("", command);
                }
                Trace.WriteLine($"ProtocolRegistration RegisterForCurrentUser {scheme}:// -> {exe}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ProtocolRegistration RegisterForCurrentUser Failed: {ex}");
            }
        }
    }
}
