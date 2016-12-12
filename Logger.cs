using System;
using System.IO;

namespace twin_db
{
    public static class Logger
    {
        private static readonly Object lockObject = new Object();
        private static readonly StreamWriter sw;

        static Logger()
        {         
            sw = new StreamWriter(File.Open("/home/pkoucky/Dokumenty/just_random/twin_DB/error.log", FileMode.Append));
        }

        public static void Log(string toLog)
        {
            lock(lockObject)
            {
                sw.WriteLine("[{0}] {1}", DateTime.Now.ToString(), toLog);
                sw.Flush();
            }
        }
    }
}