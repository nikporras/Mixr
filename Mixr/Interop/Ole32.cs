using System.Runtime.InteropServices;

namespace Mixr.Interop
{
    static class Ole32
    {
        [DllImport("ole32.dll", PreserveSig = false)]
        public static extern void PropVariantClear(ref PropVariant pvar);
    }
}
