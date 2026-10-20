using Mixr.Actions;
using Mixr.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mixr.Extensibility.Hosting
{
    public class AddonManager
    {
        public static AddonHost Host { get; } = new AddonHost();

        private static readonly AddonResolver s_resolver = new AddonResolver();
        private static readonly List<string> s_loadedAddonIds = new List<string>();
        private static bool s_shouldLoadInternalAddons = false;

        public static void Load(bool shouldLoadInternalAddons = false)
        {
            s_shouldLoadInternalAddons = shouldLoadInternalAddons;

            Host.Addons = new List<MixrAddon>();
            var loadedCatalogs = s_resolver.Load(Host);
            {
                foreach (var addon in Host.Addons.ToArray())
                {
                    try
                    {
                        ((IAddonInternal)addon).Initialize();
                        s_loadedAddonIds.Add(addon.Manifest.Id);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"AddonManager Load: Failed to initialize addon: {addon.GetType().Assembly.FullName} {ex}");
                        Host.Addons.Remove(addon);
                    }
                }
            }

            if (s_shouldLoadInternalAddons)
            {
                LoadInternalAddons();
            }

            Host.Events = Host.Addons.Where(a => a is IMixrAddonEvents).Select(a => (IMixrAddonEvents)a).ToList();
            Host.TrayContextMenuItems = Host.Addons.Where(a => a is IMixrAddonNotificationAreaContextMenu).Select(a => (IMixrAddonNotificationAreaContextMenu)a).ToList();
            Host.AppContentItems = Host.Addons.Where(a => a is IMixrAddonAppContent).Select(a => (IMixrAddonAppContent)a).ToList();
            Host.DeviceContentItems = Host.Addons.Where(a => a is IMixrAddonDeviceContent).Select(a => (IMixrAddonDeviceContent)a).ToList();
            Host.SettingsItems = Host.Addons.Where(a => a is IMixrAddonSettingsPage).Select(a => (IMixrAddonSettingsPage)a).ToList();

            Host.Events.ForEachNoThrow(x => x.OnAddonEvent(AddonEventKind.InitializeAddon));
            Host.Events.ForEachNoThrow(x => x.OnAddonEvent(AddonEventKind.AddonsInitialized));
        }

        private static void LoadInternalAddons()
        {
            var actions = new MixrActionsAddon();
            Host.Addons.Add(actions);
            ((IAddonInternal)actions).InitializeInternal(new AddonManifest { Id = "Mixr.Actions" });
        }

        public static void Shutdown()
        {
            Trace.WriteLine($"AddonManager Shutdown");
            Host.Events.ForEachNoThrow(x => x.OnAddonEvent(AddonEventKind.AppShuttingDown));
        }

        public static string GetDiagnosticInfo() => string.Join(" ", s_loadedAddonIds);
    }
}