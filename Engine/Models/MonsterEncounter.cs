using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Models
{
    public class MonsterEncounter
    {
        public int MonsterID { get; set; }
        public int EncounterRate { get; set; }

        public MonsterEncounter(int monsterID, int encounterRate)
        {
            MonsterID = monsterID;
            EncounterRate = encounterRate;
        }
    }
}
