using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApplication
{
    public static class DBAccess
    {

        public static void SaveCharacterNameList(IEnumerable<Character> toSave, string URL)
        {
            Console.WriteLine("save started {0}", URL);
            
            Console.WriteLine("save ended {0}", URL);
        }

        public static void SaveGuildNameList(IEnumerable<Guild> toSave, string URL)
        {
            Console.WriteLine("save started {0}", URL);
            
            Console.WriteLine("save ended {0}", URL);
        }
    }
}