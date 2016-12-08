﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//  metadata=res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl;provider=System.Data.SqlClient;provider connection string="data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\DB\twin_DB.mdf;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework"

namespace twin_db
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NameList nl = new NameList();

            nl.MakeCharacterNameListAsync();
            nl.MakeGuildNameListAsync();

            Console.ReadLine();
        }
    }

    public class NameList
    {
        private string urlBase1 = "http://armory.twinstar.cz/search.xml?searchQuery=";
        private string urlCharSuffix = "&searchType=character";
        private string urlGuildSuffix = "&selectedTab=guilds";

        public NameList() {}

        public async Task MakeGuildNameListAsync()
        {
            DateTime start = DateTime.Now;
            Downloader<Guild> d = new Downloader<Guild>(3, Parser.GuildNameListParser, DBAccess.SaveGuildNameList);
            List<string> URLs = new List<string>();
            int safeGuard = 3;
            int URLCount = 0;

            Console.WriteLine("GUILDS STARTED!");
            
            URLs = InitURLs(urlBase1, urlGuildSuffix);

            while (URLs.Count > 0 && safeGuard-- > 0)
            {
                URLCount += URLs.Count;
                //Console.WriteLine("to download: {0}, sum: {1}", URLs.Count.ToString(), URLCount.ToString());

                Task<List<Tuple<string,bool>>> nameListTask = d.StartDownloadAsync(null, URLs);

                await nameListTask;

                //Console.WriteLine("DOWNLOADED!");
                URLs.Clear();
                foreach(Tuple<string,bool> res in nameListTask.Result)
                {
                    if (!res.Item2)
                    {
                        URLs.AddRange(ExpandURL(res.Item1, urlGuildSuffix));
                    }
                }
                URLs = URLs.Distinct().ToList();
            }
            Console.WriteLine("GUILDS DONE! elapsed time: {0}", DateTime.Now.Subtract(start).ToString());
        }

        public async Task MakeCharacterNameListAsync()
        {
            DateTime start = DateTime.Now;
            Downloader<Character> d = new Downloader<Character>(3, Parser.CharacterNameListParser, DBAccess.SaveCharacterNameList);
            List<string> URLs;
            int safeGuard = 3;
            int URLCount = 0;

            Console.WriteLine("CHARACTERS STARTED!");

            URLs = InitURLs(urlBase1, urlCharSuffix);

            while (URLs.Count > 0 && safeGuard-- > 0)
            {
                URLCount += URLs.Count;
                //Console.WriteLine("to download: {0}, sum: {1}", URLs.Count.ToString(), URLCount.ToString());

                Task<List<Tuple<string,bool>>> nameListTask = d.StartDownloadAsync(null, URLs);

                await nameListTask;

                //Console.WriteLine("DOWNLOADED!");
                URLs.Clear();
                foreach(Tuple<string,bool> res in nameListTask.Result)
                {
                    if (!res.Item2)
                    {
                        URLs.AddRange(ExpandURL(res.Item1, urlCharSuffix));
                    }
                }
                URLs = URLs.Distinct().ToList();
            }
            Console.WriteLine("CHARACTERS DONE! elapsed time: {0}", DateTime.Now.Subtract(start).ToString());
        }

        private List<string> InitURLs(string prefix, string suffix)
        {
            List<string> URLs = new List<string>();

            for (char cFirst = 'a'; cFirst <= 'z'; cFirst++)
            {
                for (char cSecond = 'a'; cSecond <= 'z'; cSecond++)
                {
                     URLs.Add(prefix + cFirst + cSecond + suffix);
                }
            }

            return URLs;
        }
        private string ParseOutToken(string input)
        {
            return input.Substring(49, input.IndexOf('&', 49) - 49);
        }
        private List<string> ExpandURL(string URL, string suffix)
        {
            List<string> output = new List<string>();

            for (char ex = 'a'; ex <= 'z'; ex++)
            {
                output.Add(urlBase1 + ParseOutToken(URL) + ex + suffix);
            }
            return output;
        }
    }
}
