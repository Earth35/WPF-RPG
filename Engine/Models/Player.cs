using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Engine.Models
{
    public class Player : LivingEntity
    {
        private string _characterClass;
        private int _experience;

        public string CharacterClass
        {
            get { return _characterClass; }
            set
            {
                _characterClass = value;
                OnPropertyChanged(nameof(CharacterClass));
            }
        }

        public int Experience
        {
            get { return _experience; }
            private set
            {
                _experience = value;
                OnPropertyChanged(nameof(Experience));

                SetLevelAndMaximumHitPoints();
            }
        }

        public ObservableCollection<QuestStatus> Quests { get; set; }

        public Player (string name, string characterClass, int experience, int maximumHitPoints, int currentHitPoints, int gold) : base (name, maximumHitPoints, currentHitPoints, gold)
        {
            CharacterClass = characterClass;
            Experience = experience;

            Quests = new ObservableCollection<QuestStatus>();
        }

        public event EventHandler OnLeveledUp;

        public bool HasAllTheseItems(List<ItemQuantity> items)
        {
            foreach (ItemQuantity item in items)
            {
                if (Inventory.Count(ii => ii.ItemTypeID == item.ItemID) < item.Quantity)
                {
                    return false;
                }
            }

            return true;
        }

        public void GainExperience (int experience)
        {
            Experience += experience;
        }

        private void SetLevelAndMaximumHitPoints ()
        {
            int initialLevel = Level;
            Level = Experience / 100 + 1;

            if (Level != initialLevel)
            {
                MaximumHitPoints = Level * 10;

                OnLeveledUp?.Invoke(this, System.EventArgs.Empty);
            }
        }
    }
}
