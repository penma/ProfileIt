using ICities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProfileIt
{
    public class SimstepStats : ThreadingExtensionBase
    {
        private long lastStatTick = 0;

        public override void OnCreated(IThreading threading)
        {
            base.OnCreated(threading);
            lastStatTick = Stopwatch.GetTimestamp();
        }

        public override void OnAfterSimulationTick()
        {
            base.OnAfterSimulationTick();
            long tickNow = Stopwatch.GetTimestamp();
            /* stats every ~2 seconds */
            if (tickNow - lastStatTick >= Stopwatch.Frequency * 2f)
            {
                PeriodicStats();
                lastStatTick = tickNow;
            }
        }

        private void PeriodicStats()
        {
            IDictionary<string, ProfileData.MethodProfileData> pd = ProfileData.Reset();

            long ticksTotalOverall = pd.Sum(x => x.Value.ticksTotal);
            Logger.Log(String.Format("{0,-30} | {1,5}% | {2,12} | {3,10}us | {4,8}c",
                "Name", "Incl", "TickIncl", "Incl", "Call#"
                ));
            foreach (KeyValuePair<String, ProfileData.MethodProfileData> fd in pd.OrderByDescending(x => x.Value.ticksTotal))
            {
                Logger.Log(String.Format("{0,-30} | {1,5:F2}% | {2,12} | {3,10:F4}us | {4,8}c",
                    fd.Key,
                    (100f * fd.Value.ticksTotal) / ticksTotalOverall,
                    fd.Value.ticksTotal,
                    (fd.Value.ticksTotal * 1000000f) / Stopwatch.Frequency,
                    fd.Value.calls
                    ));
            }
            Logger.Log("");
        }
    }
}
