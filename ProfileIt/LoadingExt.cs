using System;
using System.Reflection;
using ColossalFramework;
using ICities;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using ColossalFramework.Math;
using Harmony;
using System.Diagnostics;

namespace ProfileIt
{
    /* TODO: This might not allow us to profile everything */
    public class LoadingExt : LoadingExtensionBase
    {
        public LoadingExt()
        {
        }

        private HarmonyInstance harmony;
        private HarmonyMethod simPre;
        private HarmonyMethod simPost;

        

        private static void PreProfiledMethod(ref ProfileData.ProfileState __state)
        {
            ProfileData.PreProfile(out __state);
        }

        private static void PostProfiledMethod(ref ProfileData.ProfileState __state)
        {
            ProfileData.PostProfile(ref __state);
        }

        private void HookMethod(MethodInfo method)
        {
            Logger.Log("      Hooking method " + method.Name);
            try
            {
                if (harmony.IsPatched(method) != null)
                {
                    Logger.Log("    " + method.Name + " is already patched!!");
                }
                else
                {
                    harmony.Patch(method, simPre, simPost);
                }
            } catch (Exception e)
            {
                Logger.Log("    Failed to patch " + method.Name + ":\n" + e.ToString());
            }
        }

        private void HookModule(Module module)
        {
            Logger.Log("Hooking all of module " + module.ScopeName);
            foreach (Type type in module.GetTypes())
            {
                if (type.FullName.Contains("`") || type.FullName != "TrafficManager.Manager.Impl.SpeedLimitManager")
                {
                    // no idea  what it is, but it breaks, TODO check later
                    Logger.Log("   Skipping type " + type.FullName);
                    continue;
                }
                Logger.Log("   Hooking type " + type.FullName);
                foreach (MethodInfo mi in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
                {
                    HookMethod(mi);
                }
            }
        }

        private void StartHarmony()
        {
            Logger.Log("Initializing Harmony");

            harmony = HarmonyInstance.Create("de.penma.citiesskylines.profileit");

            /* Patch SimulationStep of all managers */

            simPre = new HarmonyMethod(typeof(LoadingExt).GetMethod("PreProfiledMethod", BindingFlags.NonPublic | BindingFlags.Static));
            simPost = new HarmonyMethod(typeof(LoadingExt).GetMethod("PostProfiledMethod", BindingFlags.NonPublic | BindingFlags.Static));
            if (simPre == null || simPost == null)
            {
                Logger.Log("E: simPre/simPost hooks not found, exiting");
                return;
            }

            FieldInfo fiManagers = typeof(SimulationManager).GetField("m_managers", BindingFlags.NonPublic | BindingFlags.Static);
            if (fiManagers == null)
            {
                Logger.Log("E: no fieldInfo for SimulationManager.m_managers");
                return;
            }
            FastList<ISimulationManager> m_managers = (FastList<ISimulationManager>)fiManagers.GetValue(null);
            if (fiManagers == null)
            {
                Logger.Log("E: couldn't obtain SimulationManager.m_managers");
                return;
            }
            for (int j = 0; j < m_managers.m_size; j++)
            {
                ISimulationManager sm = m_managers.m_buffer[j];
                Logger.Log(String.Format("Patching SimulationStep of {0} ({1}/{2})",
                    sm.GetName(),
                    j,
                    m_managers.m_size
                    ));
                MethodInfo orig = sm.GetType().GetMethod("SimulationStep");
                HookMethod(orig);
            }

            /* Hook all of TMPE for the lulz */
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Module m in a.GetLoadedModules())
                {
                    if (m.ScopeName == "TrafficManager.dll")
                    {
                        HookModule(m);
                    }
                }
            }

            Logger.Log("Functions patched");
        }

        private void StopHarmony()
        {
            /* TODO */
        }
        
        public override void OnLevelUnloading()
        {
            StopHarmony();
            base.OnLevelUnloading();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            Logger.Init();
            Logger.Log(String.Format("Stopwatch precision: {0} ticks per second or {1:F3} microseconds resolution",
                Stopwatch.Frequency, 1000f / Stopwatch.Frequency
                ));

            StartHarmony();
        }

        
    }
}
