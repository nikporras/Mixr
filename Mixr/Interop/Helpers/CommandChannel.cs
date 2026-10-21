using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Mixr.Interop.Helpers
{
    // Mixr: passes command line arguments from a second Mixr.exe to the running one. The running Mixr
    // (packaged or not) owns a message-only window; the second one finds it and sends WM_COPYDATA.
    class CommandChannel : IDisposable
    {
        private const string WindowName = "Mixr.CommandChannel-0e510f7b-aed2-40b0-ad72-d2d3fdc89a02";
        private const int WM_COPYDATA = 0x004A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;
        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);
        private static readonly IntPtr CommandMagic = new IntPtr(0x4D495852); // "MIXR"

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, IntPtr wParam, ref COPYDATASTRUCT lParam, uint flags, uint timeout, out IntPtr result);

        private readonly HwndSource _source;
        private readonly Action<string[]> _received;

        public CommandChannel(Action<string[]> received)
        {
            _received = received;
            _source = new HwndSource(new HwndSourceParameters(WindowName)
            {
                ParentWindow = HWND_MESSAGE,
                WindowStyle = 0,
            });
            _source.AddHook(WndProc);
        }

        public void Dispose() => _source.Dispose();

        // Sends the arguments to the running Mixr. Waits up to the timeout for it to finish starting.
        public static bool Send(string[] args, TimeSpan timeout)
        {
            var text = string.Join("\n", args);
            var stopwatch = Stopwatch.StartNew();
            while (true)
            {
                var hwnd = FindWindowEx(HWND_MESSAGE, IntPtr.Zero, null, WindowName);
                if (hwnd != IntPtr.Zero)
                {
                    var data = Marshal.StringToHGlobalUni(text);
                    try
                    {
                        var copyData = new COPYDATASTRUCT { dwData = CommandMagic, cbData = text.Length * 2, lpData = data };
                        var sent = SendMessageTimeout(hwnd, WM_COPYDATA, IntPtr.Zero, ref copyData, SMTO_ABORTIFHUNG, 5000, out var result) != IntPtr.Zero;
                        Trace.WriteLine($"CommandChannel Send sent={sent} result={result}");
                        return sent && result != IntPtr.Zero;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(data);
                    }
                }

                if (stopwatch.Elapsed > timeout)
                {
                    Trace.WriteLine("CommandChannel Send No running Mixr window found");
                    return false;
                }
                Thread.Sleep(100);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                var copyData = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);
                if (copyData.dwData == CommandMagic)
                {
                    var args = Marshal.PtrToStringUni(copyData.lpData, copyData.cbData / 2).Split('\n');
                    // Return to the sender at once and run the command afterwards.
                    _source.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => _received(args)));
                    handled = true;
                    return new IntPtr(1);
                }
            }
            return IntPtr.Zero;
        }
    }
}
