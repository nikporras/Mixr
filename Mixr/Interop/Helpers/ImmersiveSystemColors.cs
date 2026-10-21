using Mixr.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace Mixr.Interop.Helpers
{
    public class ImmersiveSystemColors
    {
        public static Color Lookup(string name)
        {
            TryLookup(name, out var ret);
            return ret;
        }

        // Mixr: the dev build uses red instead of the Windows accent colour, so it's easy to tell from the stable copy.
        public static bool UseDevAccent { get; set; }
        public static readonly Color DevAccent = Color.FromRgb(0xE8, 0x11, 0x23);

        private static readonly Dictionary<string, Color> s_devAccentShades = new Dictionary<string, Color>
        {
            { "ImmersiveSystemAccent", DevAccent },
            { "ImmersiveSystemAccentLight1", Color.FromRgb(0xEF, 0x4A, 0x56) },
            { "ImmersiveSystemAccentLight2", Color.FromRgb(0xF4, 0x7F, 0x88) },
            { "ImmersiveSystemAccentLight3", Color.FromRgb(0xF9, 0xB4, 0xB9) },
            { "ImmersiveSystemAccentDark1", Color.FromRgb(0xC5, 0x0F, 0x1F) },
            { "ImmersiveSystemAccentDark2", Color.FromRgb(0xA4, 0x0E, 0x1A) },
            { "ImmersiveSystemAccentDark3", Color.FromRgb(0x75, 0x0B, 0x1C) },
        };

        public static bool TryLookup(string name, out Color color)
        {
            if (!TryLookupSystem(name, out color))
            {
                return false;
            }

            // Mixr: swap any colour that is one of the accent shades, so colours Windows derives from the accent turn red too.
            if (UseDevAccent)
            {
                if (s_devAccentShades.TryGetValue(name, out var devColor))
                {
                    color = devColor;
                    return true;
                }

                foreach (var shade in s_devAccentShades)
                {
                    if (TryLookupSystem(shade.Key, out var accentColor) && accentColor.R == color.R && accentColor.G == color.G && accentColor.B == color.B)
                    {
                        color = Color.FromArgb(color.A, shade.Value.R, shade.Value.G, shade.Value.B);
                        break;
                    }
                }
            }
            return true;
        }

        private static bool TryLookupSystem(string name, out Color color)
        {
            color = default(Color);

            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);
            var colorType = Uxtheme.GetImmersiveColorTypeFromName(name);
            var rawColor = Uxtheme.GetImmersiveColorFromColorSetEx(colorSet, colorType, false, 0);

            color = rawColor.ToABGRColor();
            return (rawColor != 4294902015);
        }

        public static IDictionary<string, Color> GetList()
        {
            var colors = new Dictionary<string, Color>();
            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);

            for (uint i = 0; ; i++)
            {
                var ptr = Uxtheme.GetImmersiveColorNamedTypeByIndex(i);
                if (ptr == IntPtr.Zero)
                {
                    break;
                }

                var name = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptr));
                TryLookup($"Immersive{name}", out var color);
                colors.Add(name, color);
            }
            return colors;
        }
    }
}
