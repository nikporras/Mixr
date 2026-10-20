using Mixr.Extensions;
using Mixr.Interop.MMDeviceAPI;
using System;

namespace Mixr.Interop.Helpers
{
    public class AudioPolicyConfigFactory
    {
        public static IAudioPolicyConfigFactory Create()
        {
            if (Environment.OSVersion.IsAtLeast(OSVersions.Version21H2))
            {
                return new AudioPolicyConfigFactoryImplFor21H2();
            }
            else
            {
                return new AudioPolicyConfigFactoryImplForDownlevel();
            }
        }
    }
}
