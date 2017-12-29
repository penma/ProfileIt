using ICities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

        private String goodMethodName(MethodBase mb)
        {
            /* TODO: Arguments etc. */
            return mb.DeclaringType.FullName + "#" + mb.Name;
        }

        private void PeriodicStats()
        {
            IDictionary<MethodBase, List<ProfileData.MethodProfileData>> pd = ProfileData.Reset();

            long ticksTotalOverall = pd.Sum(x => x.Value.Sum(y => y.ticksTotal));
            Logger.Log(String.Format("{0,-60} -> {5,-60} | {1,5}% | {2,12} | {3,10}us | {4,8}c",
                "Name", "Incl", "TickIncl", "Incl ", "Call# ", "Caller"
                ));
            foreach (KeyValuePair<MethodBase, List<ProfileData.MethodProfileData>> fd in pd.OrderByDescending(x => x.Value.Sum(y => y.ticksTotal)))
            {
                String calleeName = goodMethodName(fd.Key);
                foreach (ProfileData.MethodProfileData md in fd.Value)
                {
                    String callerName = goodMethodName(md.caller);
                    Logger.Log(String.Format("{0,-60} -> {5,-60} | {1,5:F2}% | {2,12} | {3,10:F2}us | {4,8}c",
                        calleeName,
                        (100f * md.ticksTotal) / ticksTotalOverall,
                        md.ticksTotal,
                        (md.ticksTotal * 1000000f) / Stopwatch.Frequency,
                        md.calls,
                        callerName
                        ));
                }
            }
            Logger.Log("");
        }
    }
}
