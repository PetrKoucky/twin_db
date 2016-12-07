using System;

namespace ConsoleApplication
{
    public class Character
    {
        private int ap;
        private int classId;
        private int factionId;
        private int genderId;
        private string guild;
        private int hk;
        private int level;
        private string name;
        private int raceId;

        public Character()
        {
        }

        public Character(string name, int level, int classId, int raceId, int genderId, int factionId, string guild)
        {
            this.name = name;
            this.level = level;
            this.classId = classId;
            this.raceId = raceId;
            this.genderId = genderId;
            this.factionId = factionId;
            this.guild = guild;
        }

        public Character(string name, int level, int classId, int raceId, int genderId, int factionId, string guild, int ap) : this(name, level, classId, raceId, genderId, factionId, guild)
        {
            this.ap = ap;
        }

        public Character(string name, int level, int classId, int raceId, int genderId, int factionId, string guild, int ap, int hk) : this(name, level, classId, raceId, genderId, factionId, guild, ap)
        {
            this.hk = hk;
        }
    }
}