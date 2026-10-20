using System;
using System.Collections.Generic;
using System.Windows;

namespace Mixr.Extensibility.Shared
{
    public class ResourceLoader
    {
        static List<string> _namespaces = new List<string>();

        public static void Load(string addonNamespace, bool isInternal)
        {
            if (!_namespaces.Contains(addonNamespace))
            {
                _namespaces.Add(addonNamespace);

                if (isInternal)
                {
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri($"/Mixr;component/Addons/{addonNamespace}/AddonResources.xaml", UriKind.RelativeOrAbsolute)
                    });
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri($"/{addonNamespace};component/AddonResources.xaml", UriKind.RelativeOrAbsolute)
                    });
                }
            }
        }
    }
}
