namespace Mixr.Extensibility
{
    public enum AddonEventKind
    {
        InitializeAddon,
        AddonsInitialized,
        AppShuttingDown,
    }

    public interface IMixrAddonEvents
    {
        void OnAddonEvent(AddonEventKind evt);
    }
}