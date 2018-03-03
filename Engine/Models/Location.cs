using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Factories;

namespace Engine.Models
{
    public class Location
    {
        public int XCoordinate { get; set; }
        public int YCoordinate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageName { get; set; }
        public List<Quest> QuestsAvailableHere { get; set; } = new List<Quest>();
        public List<MonsterEncounter> MonstersHere { get; set; } = new List<MonsterEncounter>();
        public Trader TraderHere { get; set; }

        public void AddMonster (int monsterID, int encounterRate)
        {
            if (MonstersHere.Exists(m => m.MonsterID == monsterID))
            {
                // This monster is already available at this location, so overwrite its encounter rate with the new value
                MonstersHere.First(m => m.MonsterID == monsterID).EncounterRate = encounterRate;
            }
            else
            {
                MonstersHere.Add(new MonsterEncounter(monsterID, encounterRate));
            }
        }

        public Monster GetMonster ()
        {
            if (!MonstersHere.Any())
            {
                // No monsters here
                return null;
            }
            else
            {
                // Total encounter rate for all monsters in this location
                int totalEncounterPercentage = MonstersHere.Sum(m => m.EncounterRate);

                // Select a random number in range
                int randomNumber = RandomNumberGenerator.NumberBetween(1, totalEncounterPercentage);

                // Loop through the monster list, 
                // adding the monster's percentage chance of appearing to the runningTotal variable.
                // When the random number is lower than the runningTotal,
                // that is the monster to return.
                int runningTotal = 0;

                foreach (MonsterEncounter monsterEncounter in MonstersHere)
                {
                    runningTotal += monsterEncounter.EncounterRate;

                    if (randomNumber <= runningTotal)
                    {
                        return MonsterFactory.GetMonster(monsterEncounter.MonsterID);
                    }
                }
            }

            // if a problem occured, return last monster in the collection
            return MonsterFactory.GetMonster(MonstersHere.Last().MonsterID);
        }
    }
}
