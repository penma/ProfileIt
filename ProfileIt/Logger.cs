using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ProfileIt
{
    class Logger
    {
        private static object logLock = new object();
        private static string logFilename = Path.Combine(Application.dataPath, "ProfileIt.log");
        private static StreamWriter writer = null;

        public static void Init()
        {
            if (writer == null)
            {
                writer = File.AppendText(logFilename);
            }
            else
            {
                Log("(Reopening log file)");
            }
        }

        public static void Close()
        {
            writer.Close();
            writer = null;
        }

        public static void Log(String m)
        {
            try
            {
                if (writer == null) { Init(); }
                Monitor.Enter(logLock);
                writer.WriteLine(m);
                writer.Flush();
            }
            finally
            {
                Monitor.Exit(logLock);
            }
        }
    }
}
