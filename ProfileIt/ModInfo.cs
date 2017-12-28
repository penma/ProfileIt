using ICities;

using System;

using ColossalFramework;
using ColossalFramework.UI;

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
    }
}
