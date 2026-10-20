using Mixr.Actions;
using Mixr.Extensibility;

namespace Mixr.Extensions
{
    public static class MixrAddonExtensions
    {
        public static bool IsInternal(this MixrAddon addon)
        {
            return addon is MixrActionsAddon;
        }
    }
}
