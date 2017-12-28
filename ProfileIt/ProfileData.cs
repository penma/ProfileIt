using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProfileIt
{
    class ProfileData
    {
        public class MethodProfileData
        {
            public long ticksTotal;
            public long calls;
        }

        internal static Dictionary<String, MethodProfileData> profileData = null;

        public static  IDictionary<String, MethodProfileData> Reset()
        {
            IDictionary<String, MethodProfileData> lastData = profileData;
            profileData = new Dictionary<String, MethodProfileData>();
            if (lastData == null)
            {
                return new Dictionary<String, MethodProfileData>();
            }
            else
            {
                return lastData;
            }
        }

        public static void AddProfileIncl(String method, ref Stopwatch sw)
        {
            if (profileData == null)
            {
                Reset();
            }

            MethodProfileData md;
            if (profileData.ContainsKey(method))
            {
                md = profileData[method];
            } else
            {
                md = new MethodProfileData();
                profileData.Add(method, md);
            }

            md.calls++;
            md.ticksTotal += sw.ElapsedTicks;
        }
    }
}
