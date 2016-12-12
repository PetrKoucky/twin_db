using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace twin_db
{
    public static class Parser
    {
        //view-source:http://armory.twinstar.cz/character-achievements.xml?r=Artemis,Artemis,Artemis&n=Sed,Sedara,Zanny&c=92
        public static IEnumerable<Character> CharacterAchievParser(WebPage wp)
        {
            XDocument xdoc;
            List<Character> parsed = new List<Character>();
            List<string> names = null;
            Character temp = null;
            Achievement tempAchiev = null;
            EarnedCAchievement tempEarned = null;

            if (wp.OK)
            {
                names = GetCharacterNames(wp.URL);
                if (names != null)
                {
                    foreach (string n in names)
                    {
                        temp = new Character();
                        temp.Name = n;
                        parsed.Add(temp);
                    }

                    try
                    {
                        xdoc = XDocument.Parse(wp.content);

                        IEnumerable<XElement> rows = from el in xdoc.Descendants("tr")
                            where (string)el.Attribute("class") == "comp_row"
                            select el;

                        foreach (XElement row in rows)
                        {
                            IEnumerable<XElement> achDescs = from el in row.Descendants("div")
                                where (string)el.Attribute("class") == "compare_desc"
                                select el;
                            IEnumerable<XElement> achDesc = from el in achDescs.First().Descendants("div")
                                where el.HasAttributes 
                                select el;                            
                            tempAchiev = new Achievement();
                            tempAchiev.Name = achDesc.First().Value.ToString();

                            IEnumerable<XElement> achs = from el in row.Descendants("td")
                                where ((string)el.Attribute("class") == "p_box" || (string)el.Attribute("class") == "completed")
                                select el;

                            for (int index = 0; index < achs.Count(); index++)
                            {
                                if ((string)achs.ElementAt(index).Attribute("class") == "completed")
                                {
                                    IEnumerable<XElement> comp = from el in achs.ElementAt(index).Descendants("div")
                                        where (string)el.Attribute("class") == "comp_date"
                                        select el;

                                    tempEarned = new EarnedCAchievement();
                                    tempEarned.Timestamp = DateTime.ParseExact(comp.First().Value.ToString(), "[dd/MM/yyyy HH:mm:ss]", CultureInfo.InvariantCulture);
                                    parsed.ElementAt(index).EarnedCAchievements.Add(tempEarned);
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
            return parsed.GetRange(1, parsed.Count() - 1);
        }
        private static List<string> GetCharacterNames(string URL)
        {
            int i = URL.IndexOf("&n=");
            int o = URL.IndexOf("&c=");

            if (i <= 0 || o <= 0)
            {
                return null;
            }

            string namesPart = URL.Substring(i + 3, (o + 3) - (i + 3));

            return namesPart.Split(',').ToList();
        } 

        /*
         * view-source:http://armory.twinstar.cz/guild-achievements.xml?r=Artemis&gn=Exalted
         */
        public static IEnumerable<Guild> GuildAPParser(WebPage wp)
        {
            XDocument xdoc;
            List<Guild> parsed = new List<Guild>();

            if (wp.OK)
            {
                try
                {
                    xdoc = XDocument.Parse(wp.content);

                    IEnumerable<XElement> elems = from el in xdoc.Descendants("guildHeader")
                        select el;

                    XElement header = elems.First();

                    IEnumerable<XElement> elems2 = from el in xdoc.Descendants("c")
                        where (string)el.Attribute("categoryId") == "0"
                        select el;

                    XElement achiev = elems2.First();

                    Guild g = new Guild();
                    g.Name = (string)header.Attribute("name");
                    g.Level = int.Parse((string)header.Attribute("level"));
                    g.FactionId = int.Parse((string)header.Attribute("faction"));
                    g.AP = int.Parse((string)achiev.Attribute("points"));
                    g.Characters = null;
                    g.ForceRefresh = false;
                    g.LastRefresh = DateTime.Now;

                    parsed.Add(g);
                }
                catch
                {

                }
            }
            return parsed;
        }

        /* 
         * view-source:http://armory.twinstar.cz/guild-info.xml?r=Artemis&gn=Exalted
         */
        public static IEnumerable<Guild> GuildCharactersParser(WebPage wp)
        {
            XDocument xdoc;
            List<Guild> parsed = new List<Guild>();
            EntitySet<Character> characters = new EntitySet<Character>();

            if (wp.OK)
            {
                try
                {
                    xdoc = XDocument.Parse(wp.content);

                    IEnumerable<XElement> elems2 = from el in xdoc.Descendants("guildHeader")
                        select el;

                    XElement header = elems2.First();

                    Guild g = new Guild();
                    g.Name = (string)header.Attribute("name");
                    g.Level = int.Parse((string)header.Attribute("level"));
                    g.FactionId = int.Parse((string)header.Attribute("faction"));
                    g.AP = 0;
                    g.ForceRefresh = true;
                    g.LastRefresh = DateTime.Now;

                    IEnumerable<XElement> elems = from el in xdoc.Descendants("character")
                        select el;

                    foreach(XElement character in elems)
                    {
                        Character c = new Character();

                        c.Name = (string)character.Attribute("name");
                        c.Level = int.Parse((string)character.Attribute("level"));
                        c.ClassId = int.Parse((string)character.Attribute("classId"));
                        c.RaceId = int.Parse((string)character.Attribute("raceId"));
                        c.GenderId = int.Parse((string)character.Attribute("genderId"));
                        c.FactionId = g.FactionId;
                        c.AP = int.Parse((string)character.Attribute("achPoints"));
                        c.HK = 0;
                        c.ForceRefresh = true;
                        c.LastRefresh = DateTime.Now;

                        c.Guild = g;

                        characters.Add(c);
                    }
                    g.Characters = characters;
                    parsed.Add(g);
                }
                catch
                {
                    Console.WriteLine("error in guild characters parsing of " + wp.URL);
                }
            }

            return parsed;
        }

        /*
         * view-source:http://armory.twinstar.cz/character-feed.xml?r=Artemis&cn=Sedara
         * characters without a guild
         */
        public static IEnumerable<Character> CharacterFeedParser(WebPage wp)
        {
            XDocument xdoc;
            List<Character> parsed = new List<Character>();

            if (wp.OK)
            {
                try
                {
                    xdoc = XDocument.Parse(wp.content);

                    IEnumerable<XElement> elems = from el in xdoc.Descendants("character")
                        select el;

                    XElement character = elems.First();

                    Character c = new Character();

                    c.Name = (string)character.Attribute("name");
                    c.Level = int.Parse((string)character.Attribute("level"));
                    c.ClassId = int.Parse((string)character.Attribute("classId"));
                    c.RaceId = int.Parse((string)character.Attribute("raceId"));
                    c.GenderId = int.Parse((string)character.Attribute("genderId"));
                    c.FactionId = int.Parse((string)character.Attribute("factionId"));
                    c.AP = int.Parse((string)character.Attribute("points"));
                    c.HK = int.Parse((string)character.Attribute("kills"));
                    c.ForceRefresh = false;
                    c.LastRefresh = DateTime.Now;
                    c.Guild = null;

                    parsed.Add(c);
                }
                catch
                {
                    Console.WriteLine("error in character feed parsing of " + wp.URL);
                }
            }

            return parsed;
        }

        /*
         * http://armory.twinstar.cz/search.xml?searchQuery=aa&selectedTab=guilds   
         */
        public static IEnumerable<Guild> GuildNameListParser(WebPage wp)
        {
            //Console.WriteLine("parse started {0}", wp.URL);

            List<Guild> parsed = new List<Guild>();
            XDocument xdoc;
            
            if (wp.OK) //sanity check
            {
                try
                {
                    xdoc = XDocument.Parse(wp.content);
                    IEnumerable<string> sGuildNames = from el in xdoc.Descendants("guild")
                        where (string)el.Attribute("realm") == "Artemis"
                        select (string)el.Attribute("name");
                    
                    foreach(string s in sGuildNames)
                    {
                        Guild g = new Guild();
                        g.Name = s;
                        g.Level = 0;
                        g.FactionId = 2;
                        g.AP = 0;
                        g.ForceRefresh = true;
                        g.LastRefresh = DateTime.Now;

                        parsed.Add(g);
                    }
                }
                catch
                {
                    Console.WriteLine("error in guild parsing of " + wp.URL);
                }
            }

            //Console.WriteLine("parse ended {0}, total guilds parsed {1}", wp.URL, parsed.Count.ToString());
            return parsed;
        }

        /*
         * http://armory.twinstar.cz/search.xml?searchQuery=aa&searchType=character
         */
        public static IEnumerable<Character> CharacterNameListParser(WebPage wp)
        {
            //Console.WriteLine("parse started {0}", wp.URL);

            List<Character> parsed = new List<Character>();
            XDocument xdoc;
            
            if (wp.OK) //sanity check
            {
                try
                {
                    xdoc = XDocument.Parse(wp.content);
                    IEnumerable<XElement> characters = from el in xdoc.Descendants("character")
                        where (string)el.Attribute("realm") == "Artemis"
                        select el;
                    
                    foreach(XElement character in characters)
                    {
                        Character c = new Character();
                        Guild g = null;
                        string gName = "";

                        c.Name = (string)character.Attribute("name");
                        c.Level = int.Parse((string)character.Attribute("level"));
                        c.ClassId = int.Parse((string)character.Attribute("classId"));
                        c.RaceId = int.Parse((string)character.Attribute("raceId"));
                        c.GenderId = int.Parse((string)character.Attribute("genderId"));
                        c.FactionId = int.Parse((string)character.Attribute("factionId"));
                        c.AP = 0;
                        c.HK = 0;
                        c.ForceRefresh = true;
                        c.LastRefresh = DateTime.Now;
                        gName = (string)character.Attribute("guild");
                        if (gName != null)
                        {
                            g = new Guild();
                            g.Name = gName;
                            g.Level = 0;
                            g.FactionId = c.FactionId;
                            g.AP = 0;
                            g.ForceRefresh = true;
                            g.LastRefresh = DateTime.Now;
                        }
                        c.Guild = g;

                        parsed.Add(c);
                    }
                }
                catch
                {
                    Console.WriteLine("error in characters parsing of " + wp.URL);
                }
            }

            //Console.WriteLine("parse ended {0}, total characters parsed {1}", wp.URL, parsed.Count.ToString());
            return parsed;
        }
    }
}