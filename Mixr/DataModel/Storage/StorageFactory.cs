namespace Mixr.DataModel.Storage
{
    public class StorageFactory
    {
        private static ISettingsBag s_globalSettings;

        static StorageFactory()
        {
            s_globalSettings = App.HasIdentity ? (ISettingsBag)new Internal.WindowsStorageSettingsBag() : new Internal.RegistrySettingsBag();
            // Mixr: packaged and unpackaged runs keep separate settings, so record which one this is.
            System.Diagnostics.Trace.WriteLine($"StorageFactory Using {s_globalSettings.GetType().Name} (HasIdentity={App.HasIdentity})");
        }

        public static ISettingsBag GetSettings(string nameSpace = null)
        {
            return (nameSpace == null) ? s_globalSettings :
                new Internal.NamespacedSettingsBag(nameSpace, s_globalSettings);
        }
    }
}
