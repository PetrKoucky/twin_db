using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace ConsoleApplication
{
    public static class Parser
    {
        public static IEnumerable<Character> GuildCharactersParser(WebPage wp)
        {
            XDocument xdoc;
            List<Character> parsed = new List<Character>();

            if (wp.OK)
            {
                try
                {
                    xdoc = XDocument.Parse(wp.content);

                    IEnumerable<XElement> characters = from el in xdoc.Descendants("character")
                        select el;

                    foreach(XElement character in characters)
                    {
                        string name = (string)character.Attribute("name");
                        int level = int.Parse((string)character.Attribute("level"));
                        int classId = int.Parse((string)character.Attribute("classId"));
                        int raceId = int.Parse((string)character.Attribute("raceId"));
                        int genderId = int.Parse((string)character.Attribute("genderId"));
                        int factionId = int.Parse((string)character.Attribute("factionId"));
                        int ap = int.Parse((string)character.Attribute("achPoints"));

                        parsed.Add(new Character(name, level, classId, raceId, genderId, factionId, "", ap));
                    }
                }
                catch
                {
                    Console.WriteLine("error in guild characters parsing of " + wp.URL);
                }
            }

            return parsed;
        }

        //for characters without a guild
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

                    string name = (string)character.Attribute("name");
                    int level = int.Parse((string)character.Attribute("level"));
                    int classId = int.Parse((string)character.Attribute("classId"));
                    int raceId = int.Parse((string)character.Attribute("raceId"));
                    int genderId = int.Parse((string)character.Attribute("genderId"));
                    int factionId = int.Parse((string)character.Attribute("factionId"));
                    string guild = (string)character.Attribute("guild");
                    int ap = int.Parse((string)character.Attribute("point"));
                    int hk = int.Parse((string)character.Attribute("kills"));

                    parsed.Add(new Character(name, level, classId, raceId, genderId, factionId, guild, ap, hk));
                }
                catch
                {
                    Console.WriteLine("error in character feed parsing of " + wp.URL);
                }
            }

            return parsed;
        }

        public static IEnumerable<Guild> GuildNameListParser(WebPage wp)
        {
            Console.WriteLine("parse started {0}", wp.URL);

            SortedSet<Guild> parsed = new SortedSet<Guild>();
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
                        parsed.Add(new Guild(s));
                    }
                }
                catch
                {
                    Console.WriteLine("error in guild parsing of " + wp.URL);
                }
            }

            Console.WriteLine("parse ended {0}, total guilds parsed {1}", wp.URL, parsed.Count.ToString());
            return parsed;
        }

        public static IEnumerable<Character> CharacterNameListParser(WebPage wp)
        {
            Console.WriteLine("parse started {0}", wp.URL);

            SortedSet<Character> parsed = new SortedSet<Character>();
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
                        string name = (string)character.Attribute("name");
                        int level = int.Parse((string)character.Attribute("level"));
                        int classId = int.Parse((string)character.Attribute("classId"));
                        int raceId = int.Parse((string)character.Attribute("raceId"));
                        int genderId = int.Parse((string)character.Attribute("genderId"));
                        int factionId = int.Parse((string)character.Attribute("factionId"));
                        string guild = (string)character.Attribute("guild");
                        
                        parsed.Add(new Character(name, level, classId, raceId, genderId, factionId, guild));
                    }
                }
                catch
                {
                    Console.WriteLine("error in characters parsing of " + wp.URL);
                }
            }

            Console.WriteLine("parse ended {0}, total characters parsed {1}", wp.URL, parsed.Count.ToString());
            return parsed;
        }
    }
}