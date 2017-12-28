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

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            Logger.Init();
        }

        public override void OnReleased()
        {
            Logger.Close();
            base.OnReleased();
        }

        private HarmonyInstance harmony;


        private static void PreSimulationStep(ISimulationManager __instance, ref Stopwatch __state)
        {
            __state = Stopwatch.StartNew();
        }

        private static void PostSimulationStep(ISimulationManager __instance, ref Stopwatch __state)
        {
            __state.Stop();
            ProfileData.AddProfileIncl(__instance.GetName(), ref __state);
        }

        private void StartHarmony()
        {
            Logger.Log("Initializing Harmony");

            harmony = HarmonyInstance.Create("de.penma.citiesskylines.profileit");

            /* Patch SimulationStep of all managers */

            HarmonyMethod simPre = new HarmonyMethod(typeof(LoadingExt).GetMethod("PreSimulationStep", BindingFlags.NonPublic | BindingFlags.Static));
            HarmonyMethod simPost = new HarmonyMethod(typeof(LoadingExt).GetMethod("PostSimulationStep", BindingFlags.NonPublic | BindingFlags.Static));
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
                if (harmony.IsPatched(orig) != null)
                {
                    Logger.Log("   already patched!!");
                }
                else
                {
                    harmony.Patch(orig, simPre, simPost);
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
