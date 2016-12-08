using System;
using System.Collections.Generic;
#if (WINDOWS)
using System.Data.Linq;
#endif
using System.Linq;
using System.Text;

namespace twin_db
{
    public static class DBAccess
    {
        private static string connectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\kouck\Documents\twinstar_project\DB\twin_DB.mdf;Integrated Security=True;Connect Timeout=30";
        
        
        public static void SaveCharacterNameList(IEnumerable<Character> toSave, string URL)
        {
            if (toSave == null)
                return;

            foreach(Character c in toSave)
            {
                InsertCharacter(c);
            }
        }

        public static void SaveGuildNameList(IEnumerable<Guild> toSave, string URL)
        {
            if (toSave == null)
                return;

            foreach (Guild g in toSave)
            {
                InsertGuild(g);
            }
        }

        public static IEnumerable<Guild> GetGuildSet()
        {
            #if (WINDOWS)
            twin_db_lsqlDataContext db = new twin_db_lsqlDataContext(connectionString);

            //TODO get Guild table
            #endif
            List<Guild> guilds = new List<Guild>();
            Guild g = new Guild();
            g.Name = "Exalted";
            guilds.Add(g);

            return guilds;
        }


        private static bool InsertCharacter(Character character)
        {
            #if (WINDOWS)
            twin_db_lsqlDataContext db = new twin_db_lsqlDataContext(connectionString);
            //Console.WriteLine("Character {0}", character.Name);
            
            var match =
                (from c in db.Characters
                 where c.Name == character.Name
                 select c).SingleOrDefault();

            if (match == null)
            {
                try
                {
                    Table<Character> table = db.GetTable<Character>();
                    Character cIn = new Character();
                    Guild guild;

                    cIn.Name = character.Name;
                    cIn.Level = character.Level;
                    cIn.AP = character.AP;
                    cIn.HK = character.HK;
                    cIn.FactionId = character.FactionId;
                    cIn.ClassId = character.ClassId;
                    cIn.RaceId = character.RaceId;
                    cIn.GenderId = character.GenderId;
                    cIn.LastRefresh = character.LastRefresh;
                    cIn.ForceRefresh = character.ForceRefresh;

                    if (character.Guild != null) //Is character in a Guild?
                    {
                        guild =
                            (from g in db.Guilds
                             where g.Name == character.Guild.Name
                             select g).SingleOrDefault();

                        if (guild != null) //Does the Guild exists already?
                        {
                            cIn.Guild_Id = guild.Id;
                        }
                        else
                        {
                            InsertGuild(character.Guild);
                            return InsertCharacter(character);
                        }
                    }

                    table.InsertOnSubmit(cIn);
                    table.Context.SubmitChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                try
                {
                    Guild guild;

                    match.Name = character.Name;
                    match.Level = character.Level;
                    match.AP = character.AP;
                    match.HK = character.HK;
                    match.FactionId = character.FactionId;
                    match.ClassId = character.ClassId;
                    match.RaceId = character.RaceId;
                    match.GenderId = character.GenderId;
                    match.LastRefresh = character.LastRefresh;
                    match.ForceRefresh = character.ForceRefresh;

                    if (character.Guild != null) //Is character in a Guild?
                    {
                        guild =
                            (from g in db.Guilds
                             where g.Name == character.Guild.Name
                             select g).SingleOrDefault();

                        if (guild != null) //Does the Guild exists already?
                        {
                            match.Guild_Id = guild.Id;
                        }
                        else
                        {
                            InsertGuild(character.Guild);
                            return InsertCharacter(character);
                        }
                    }

                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            
            #endif
            return true;
        }

        private static bool InsertGuild(Guild guildToInsert)
        {
            #if (WINDOWS)
            twin_db_lsqlDataContext db = new twin_db_lsqlDataContext(connectionString);
            //Console.WriteLine("Guild {0}", guildToInsert.Name);

            var match =
                (from c in db.Guilds
                 where c.Name == guildToInsert.Name
                 select c).SingleOrDefault();

            if (match == null)
            {
                try
                {
                    Table<Guild> table = db.GetTable<Guild>();
                    Guild gIn = new Guild();
                    gIn.Name = guildToInsert.Name;
                    gIn.Level = guildToInsert.Level;
                    gIn.FactionId = guildToInsert.FactionId;
                    gIn.AP = guildToInsert.AP;
                    gIn.LastRefresh = guildToInsert.LastRefresh;
                    gIn.ForceRefresh = guildToInsert.ForceRefresh;

                    table.InsertOnSubmit(gIn);
                    table.Context.SubmitChanges();

                    if (guildToInsert.Characters.Count > 0)
                    {
                        foreach (Character c in guildToInsert.Characters)
                        {
                            InsertCharacter(c);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                try
                {
                    if (guildToInsert.Level > match.Level)
                        match.Level = guildToInsert.Level;
                    if (guildToInsert.FactionId < 2)
                        match.FactionId = guildToInsert.FactionId;
                    match.LastRefresh = guildToInsert.LastRefresh;
                    match.ForceRefresh = guildToInsert.ForceRefresh;

                    if (guildToInsert.AP > match.AP)
                        match.AP = guildToInsert.AP;

                    if (guildToInsert.Characters.Count > 0)
                    {
                        foreach (Character c in guildToInsert.Characters)
                        {
                            InsertCharacter(c);
                        }
                    }

                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }

            #endif
            return true;
        }
    }
}