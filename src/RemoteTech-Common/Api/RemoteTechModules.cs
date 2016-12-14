using System.Collections.Generic;

namespace RemoteTech.Common.Api
{
    public static class RemoteTechModules
    {
        public const string RemoteTechDelayAssemblyName = "RemoteTech-Delay.dll";

        public static bool IsRemoteTechDelayLoaded()
        {
            return AssemblyByName(RemoteTechDelayAssemblyName) != null;
        }

        internal static AssemblyLoader.LoadedAssembly AssemblyByName(string assemblyName)
        {
            var assemblyCount = AssemblyLoader.loadedAssemblies.Count;
            for (var i = 0; i < assemblyCount; i++)
            {
                var assembly = AssemblyLoader.loadedAssemblies[i];
                if (assembly.name == assemblyName)
                    return assembly;
            }

            return null;
        }
    }
}
