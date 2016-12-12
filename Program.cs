using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

//  metadata=res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl;provider=System.Data.SqlClient;provider connection string="data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\DB\twin_DB.mdf;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"

namespace twin_db
{
    public class Program
    {
        private static Core core;
        private static int cursorTop;

        public static void Main(string[] args)
        {
            cursorTop = Console.CursorTop;
            core = new Core();
            Menu();
        }

        private static void Menu()
        {
            bool run = true;
            string input = "";
            int intInput = 0;
            Task running = Task.Delay(1);  
            var progressReporter1 = new Progress<Tuple<int, string>>(ProgressReport);          

            PrintHelp();
            while (run)
            {
                input = Console.ReadLine();
                switch (input)
                {
                    case "c":
                        Console.Write("Enter new concurrency level: ");
                        input = Console.ReadLine();
                        int.TryParse(input, out intInput);
                        core.concurrency = intInput;
                        break;
                    case "1":
                        if (running.IsCompleted)
                        {
                            running = core.MakeCharacterNameListAsync(progressReporter1);
                        }
                        break;
                    case "2":
                        if (running.IsCompleted)
                        {
                            running = core.MakeGuildNameListAsync(progressReporter1);
                        }
                        break;
                    case "3":
                        if (running.IsCompleted)
                        {
                            running = core.MakeGuildsAsync(progressReporter1);
                        }
                        break;
                    case "4":
                        if (running.IsCompleted)
                        {
                            running = core.UpdateGuildApAsync(progressReporter1);
                        }
                        break;
                    case "5":
                        if (running.IsCompleted)
                        {
                            running = core.UpdateCharactersAchievementsAsync(progressReporter1);
                        }
                        break;
                    case "exit":
                        run = false;
                        break;
                    default:
                        PrintHelp();
                        break;
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("c - set concurrency level");

            Console.WriteLine("1 - Download, parse and save Characters");
            Console.WriteLine("    Going throught aa -> zz");

            Console.WriteLine("2 - Download, parse and save Guilds");
            Console.WriteLine("    Going throught aa -> zz");

            Console.WriteLine("3 - Get Guilds from DB and download their characters");

            Console.WriteLine("4 - Get Guilds from DB and download their APs");

            Console.WriteLine("5 - Get Achievements of Characters");

            Console.WriteLine("exit");
        }

        public static void ProgressReport(Tuple<int, string> tup)
        {
            int left = Console.CursorLeft, top = Console.CursorTop; 
            Console.SetCursorPosition(80 , cursorTop);
            Console.Write("Started {0,3}%, {1}", tup.Item1.ToString(), tup.Item2);
            Console.SetCursorPosition(left , top);
        }
    }

    public class Core
    {
        private const string URLBASE1 = "http://armory.twinstar.cz/search.xml?searchQuery=";
        private const string URLCHARSUFFIX = "&searchType=character";
        private const string URLGUILDSUFFIX = "&selectedTab=guilds";
        private const string URLGUILDBASE = "http://armory.twinstar.cz/guild-info.xml?r=Artemis&gn=";
        private const string URLGUILDAPBASE = "http://armory.twinstar.cz/guild-achievements.xml?r=Artemis&gn=";
        private const string URLCHARACTERACHIEVEMENTS = "http://armory.twinstar.cz/character-achievements.xml?r=";
        private const string URLNAMESEARCH = "&n=";
        private const string URLCATEGORYSEARCH = "&c=";
        private const string URLREALMSEARCH = "&r=";

        private const int DEF_SAVEGUARD = 3;

        private int _concurrency;

        public Core() 
        {
            this._concurrency = 3;
        }

        public int concurrency
		{
			get
			{
				return this._concurrency;
			}
			set
			{
				if ((this._concurrency != value) && (value > 1))
				{
					this._concurrency = value;
				}
			}
		}

        public async Task UpdateCharactersAchievementsAsync(IProgress<Tuple<int, string>> progres)
        {
            DateTime start = DateTime.Now;
            Downloader<Character> d = new Downloader<Character>(this._concurrency, Parser.CharacterAchievParser, DBAccess.SaveCharacterNameList);
            List<string> URLs = new List<string>();
            int charsPerReq = 12;
            int[] cat = {92, 96, 95, 168, 81, 201, 15068};

            List<Character> chars = DBAccess.GetCharacterSet().ToList();
            //TODO get all character achievement categories

            //foreach category id
            foreach(int c in cat)
            {
                for (int i = 0; i < chars.Count; i+=charsPerReq)
                {
                    if ((chars.Count - i) < charsPerReq)
                    {
                        URLs.Add(CreateCharacterAchievURL(chars.GetRange(i, chars.Count - i), c));
                    }
                    else
                    {
                        URLs.Add(CreateCharacterAchievURL(chars.GetRange(i, charsPerReq), c));
                    }
                }
            }

            Task<List<Tuple<string,bool>>> task = d.StartDownloadAsync(progres, URLs);

            await task;

            Console.WriteLine("Done with Update of Character achievements");
        }
        private string CreateCharacterAchievURL(List<Character> characters, int category)
        {
            string names = "Sed,";
            string realm = "Artemis,";
            foreach (Character c in characters)
            {
                names += c.Name + ",";
                realm += "Artemis,";
            }
            return URLCHARACTERACHIEVEMENTS + realm.Substring(0, realm.Length - 1) + URLNAMESEARCH 
                + names.Substring(0, names.Length - 1) + URLCATEGORYSEARCH + category.ToString();
        }

        public async Task UpdateGuildApAsync(IProgress<Tuple<int, string>> progres)
        {
            DateTime start = DateTime.Now;            
            Downloader<Guild> d = new Downloader<Guild>(this._concurrency, Parser.GuildAPParser, DBAccess.SaveGuildNameList);
            List<string> URLsAP = new List<string>();

            Console.WriteLine("Update of guild achievement points STARTED!");

            foreach (Guild g in DBAccess.GetGuildSet())
            {
                URLsAP.Add(URLGUILDAPBASE + g.Name);
            }

            Task<List<Tuple<string,bool>>> guildsAPTask = d.StartDownloadAsync(progres, URLsAP);

            await guildsAPTask;

            Console.WriteLine("DONE! elapsed time {0}", (DateTime.Now.Subtract(start)).ToString());
        }

        public async Task MakeGuildsAsync(IProgress<Tuple<int, string>> progres)
        {
            DateTime start = DateTime.Now;
            Downloader<Guild> d = new Downloader<Guild>(this._concurrency, Parser.GuildCharactersParser, DBAccess.SaveGuildNameList);            
            List<string> URLs = new List<string>();            

            Console.WriteLine("Update of guild characters STARTED!");

            foreach (Guild g in DBAccess.GetGuildSet())
            {
                URLs.Add(URLGUILDBASE + g.Name);
            }

            Task<List<Tuple<string,bool>>> guildsTask = d.StartDownloadAsync(progres, URLs);

            await guildsTask;

            Console.WriteLine("DONE! elapsed time {0}", (DateTime.Now.Subtract(start)).ToString());
        }

        /*
         * Go throught all possible two char combinations and query server
         * Result is DB with all guilds from server
         * Filled informations
         *    - Name, LastRefresh
         * Missing informations
         *    - Level, FactionId, AP, characters
         * ForceUpdate = true
         */
        public async Task MakeGuildNameListAsync(IProgress<Tuple<int, string>> progres)
        {
            DateTime start = DateTime.Now;
            Downloader<Guild> d = new Downloader<Guild>(this._concurrency, Parser.GuildNameListParser, DBAccess.SaveGuildNameList);
            List<string> URLs = new List<string>();
            int safeGuard = DEF_SAVEGUARD;
            int URLCount = 0;

            Console.WriteLine("GUILDS STARTED!");
            
            URLs = InitNameListURLs(URLBASE1, URLGUILDSUFFIX);

            while (URLs.Count > 0 && safeGuard-- > 0)
            {
                URLCount += URLs.Count;

                Task<List<Tuple<string,bool>>> nameListTask = d.StartDownloadAsync(progres, URLs);

                await nameListTask;

                Console.WriteLine("\nPass {0} done!", (DEF_SAVEGUARD - safeGuard).ToString());
                URLs.Clear();
                foreach(Tuple<string,bool> res in nameListTask.Result)
                {
                    if (!res.Item2)
                    {
                        URLs.AddRange(ExpandNameListURL(res.Item1, URLGUILDSUFFIX));
                    }
                }
                URLs = URLs.Distinct().ToList();
            }
            Console.WriteLine("GUILDS DONE! elapsed time: {0}", DateTime.Now.Subtract(start).ToString());
        }

        /*
         * Go throught all possible two char combinations and query server
         * Result is DB with all characters from server
         * Filled informations
         *    - Name, Level, ClassId, GenderId, RaceId, FactionId, Guild, LastRefresh
         * Missing informations
         *    - AP, HK
         * ForceUpdate = true
         */
        public async Task MakeCharacterNameListAsync(IProgress<Tuple<int, string>> progres)
        {
            DateTime start = DateTime.Now;
            Downloader<Character> d = new Downloader<Character>(this._concurrency, Parser.CharacterNameListParser, DBAccess.SaveCharacterNameList);
            List<string> URLs;
            int safeGuard = DEF_SAVEGUARD;
            int URLCount = 0;

            Console.WriteLine("CHARACTERS STARTED!");

            URLs = InitNameListURLs(URLBASE1, URLCHARSUFFIX);

            while (URLs.Count > 0 && safeGuard-- > 0)
            {
                URLCount += URLs.Count;

                Task<List<Tuple<string,bool>>> nameListTask = d.StartDownloadAsync(progres, URLs);

                await nameListTask;

                Console.WriteLine("\nPass {0} done!", (DEF_SAVEGUARD - safeGuard).ToString());
                URLs.Clear();
                foreach(Tuple<string,bool> res in nameListTask.Result)
                {
                    if (!res.Item2)
                    {
                        URLs.AddRange(ExpandNameListURL(res.Item1, URLCHARSUFFIX));
                    }
                }
                URLs = URLs.Distinct().ToList();
            }
            Console.WriteLine("CHARACTERS DONE! elapsed time: {0}", DateTime.Now.Subtract(start).ToString());
        }

        private List<string> InitNameListURLs(string prefix, string suffix)
        {
            List<string> URLs = new List<string>();

            for (char cFirst = 'a'; cFirst <= 'b'; cFirst++)
            {
                for (char cSecond = 'a'; cSecond <= 'e'; cSecond++)
                {
                     URLs.Add(prefix + cFirst + cSecond + suffix);
                }
            }

            return URLs;
        }
        private string ParseOutNameListToken(string input)
        {
            return input.Substring(49, input.IndexOf('&', 49) - 49);
        }
        private List<string> ExpandNameListURL(string URL, string suffix)
        {
            List<string> output = new List<string>();

            for (char ex = 'a'; ex <= 'a'; ex++)
            {
                output.Add(URLBASE1 + ParseOutNameListToken(URL) + ex + suffix);
            }
            return output;
        }
    }
}
