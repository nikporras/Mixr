using System.Runtime.InteropServices;

namespace Mixr.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
    }
}
