using Mixr.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mixr.UI.Helpers
{
    public enum MixrCommandKind
    {
        Device,
        Cycle,
        Profile,
        Mic,
        Volume,
    }

    // Mixr: a command from Mixr.exe arguments (Mixr.exe device "Headphones") or a mixr:// link
    // (mixr://device/Headphones), for Stream Deck, AutoHotkey and the like.
    public class MixrCommand
    {
        public const string UriScheme = "mixr";
        public const int DefaultVolumeStep = 5;

        public MixrCommandKind Kind { get; private set; }
        public string Argument { get; private set; }

        // Returns null if the arguments aren't a known command.
        public static MixrCommand Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return null;
            }

            List<string> tokens;
            string separator;
            if (args.Length == 1 && args[0].StartsWith(UriScheme + ":", StringComparison.OrdinalIgnoreCase))
            {
                var path = args[0].Substring(UriScheme.Length + 1);
                var queryStart = path.IndexOf('?');
                if (queryStart >= 0)
                {
                    path = path.Substring(0, queryStart);
                }
                tokens = path.Split('/').Where(t => t.Length > 0).Select(Uri.UnescapeDataString).ToList();
                separator = "/";
            }
            else
            {
                tokens = args.ToList();
                separator = " ";
            }

            if (!tokens.Any())
            {
                return null;
            }

            var verb = tokens[0].TrimStart('-', '/').ToLowerInvariant();
            var argument = string.Join(separator, tokens.Skip(1)).Trim();
            switch (verb)
            {
                case "device":
                    return argument.Length > 0 ? new MixrCommand { Kind = MixrCommandKind.Device, Argument = argument } : null;
                case "cycle":
                    return new MixrCommand { Kind = MixrCommandKind.Cycle, Argument = argument };
                case "profile":
                    return argument.Length > 0 ? new MixrCommand { Kind = MixrCommandKind.Profile, Argument = argument } : null;
                case "mic":
                    return TryGetMicMute(argument, out _) ? new MixrCommand { Kind = MixrCommandKind.Mic, Argument = argument } : null;
                case "volume":
                    return TryGetVolume(argument, 0, out _) ? new MixrCommand { Kind = MixrCommandKind.Volume, Argument = argument } : null;
                default:
                    return null;
            }
        }

        // "toggle" (or nothing) gives null, "mute"/"off" gives true, "unmute"/"on" gives false.
        public static bool TryGetMicMute(string argument, out bool? isMuted)
        {
            isMuted = null;
            switch ((argument ?? "").Trim().ToLowerInvariant())
            {
                case "":
                case "toggle":
                    return true;
                case "mute":
                case "off":
                    isMuted = true;
                    return true;
                case "unmute":
                case "on":
                    isMuted = false;
                    return true;
                default:
                    return false;
            }
        }

        // "50" sets 50%, "+5"/"-5" change it by 5, "up"/"down" by DefaultVolumeStep or "up 10" by 10. Clamped to 0-100.
        public static bool TryGetVolume(string argument, int current, out int volume)
        {
            volume = current;
            var parts = (argument ?? "").Split(new[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts.Length > 2)
            {
                return false;
            }

            var first = parts[0].ToLowerInvariant();
            if (first == "up" || first == "down")
            {
                var step = DefaultVolumeStep;
                if (parts.Length == 2 && !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out step))
                {
                    return false;
                }
                volume = current + (first == "up" ? step : -step);
            }
            else if (parts.Length == 1 && int.TryParse(first.TrimEnd('%'), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
            {
                volume = first.StartsWith("+") || first.StartsWith("-") ? current + value : value;
            }
            else
            {
                return false;
            }

            volume = Math.Max(0, Math.Min(100, volume));
            return true;
        }

        // Matches the id, then the Mixr name, the Windows name or the short name ("Speakers"), ignoring case.
        // Falls back to a part of the name if exactly one device has it.
        public static DeviceViewModel FindDevice(IEnumerable<DeviceViewModel> devices, string nameOrId)
        {
            var name = nameOrId.Trim();
            var list = devices.ToList();
            bool Equal(string value) => string.Equals(value?.Trim(), name, StringComparison.OrdinalIgnoreCase);
            bool Contains(string value) => value != null && value.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0;

            var match = list.FirstOrDefault(d => Equal(d.Id))
                ?? list.FirstOrDefault(d => Equal(d.DisplayName))
                ?? list.FirstOrDefault(d => Equal(d.OriginalDisplayName))
                ?? list.FirstOrDefault(d => Equal(d.DeviceDescription));
            if (match != null)
            {
                return match;
            }

            var partial = list.Where(d => Contains(d.DisplayName) || Contains(d.OriginalDisplayName)).ToList();
            return partial.Count == 1 ? partial[0] : null;
        }

        public override string ToString() => $"{Kind} '{Argument}'";
    }
}
