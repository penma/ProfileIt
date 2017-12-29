using ICities;

using System;

using ColossalFramework;
using ColossalFramework.UI;
using System.Reflection;
using System.Reflection.Emit;

namespace ProfileIt
{
    public class ModInfo : IUserMod
    { public string Name
        {
            get { return "Profile It! " + version; }
        }

        public string Description
        {
            get { return "Find out what's making your game slow"; }
        }

        public const string version = "1.0.0";

        public void OnEnabled()
        {
            Logger.Init();

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Logger.Log(String.Format("Loaded assembly: fullname {0}, location {1}",
                    a.FullName,
                    a is AssemblyBuilder ? "(AssemblyBuilder)" : a.Location
                    ));
                foreach (Module m in a.GetLoadedModules())
                {
                    Logger.Log(String.Format("    has module {0} {1} ({2})",
                        m.Name,
                        m.ScopeName,
                        m.FullyQualifiedName
                        ));
                }
            }

        }

        public void OnDisabled()
        {
            Logger.Close();
        }
    }
}
