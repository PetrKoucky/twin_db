using System;
using System.IO;
using System.Runtime.CompilerServices;

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

        public static void Log(string toLog, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            lock(lockObject)
            {
                sw.WriteLine("[{0}] <{1}:{2}> {3}", DateTime.Now.ToString(), filePath, lineNumber.ToString(), toLog);
                sw.Flush();
            }
        }
    }
}