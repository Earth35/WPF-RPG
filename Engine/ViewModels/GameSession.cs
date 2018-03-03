using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Models;
using Engine.Factories;
using System.ComponentModel;
using Engine.EventArgs;

namespace Engine.ViewModels
{
    public class GameSession : BaseNotificationClass
    {
        public event EventHandler<GameMessageEventArgs> OnMessageRaised;

        private Location _currentLocation;
        private Monster _currentMonster;
        private Trader _currentTrader;

        #region Properties

        public Player CurrentPlayer { get; set; }
        public World CurrentWorld { get; set; }

        public Location CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value;

                OnPropertyChanged(nameof(CurrentLocation));
                OnPropertyChanged(nameof(HasLocationToNorth));
                OnPropertyChanged(nameof(HasLocationToWest));
                OnPropertyChanged(nameof(HasLocationToEast));
                OnPropertyChanged(nameof(HasLocationToSouth));

                GivePlayerQuestsAtLocation();
                CompleteQuestsAtLocation();
                GetMonsterAtLocation();

                CurrentTrader = CurrentLocation.TraderHere;
            }
        }

        public Monster CurrentMonster
        {
            get { return _currentMonster; }
            set
            {
                _currentMonster = value;
                OnPropertyChanged(nameof(CurrentMonster));
                OnPropertyChanged(nameof(HasMonster));

                if (CurrentMonster != null)
                {
                    RaiseMessage("");
                    RaiseMessage($"You see a {CurrentMonster.Name} here!");
                }
            }
        }

        public Trader CurrentTrader
        {
            get { return _currentTrader; }
            set
            {
                _currentTrader = value;
                OnPropertyChanged(nameof(CurrentTrader));
                OnPropertyChanged(nameof(HasTrader));
            }
        }

        public Weapon CurrentWeapon { get; set; }

        public bool HasLocationToNorth => CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate + 1) != null;

        public bool HasLocationToWest => CurrentWorld.LocationAt(CurrentLocation.XCoordinate - 1, CurrentLocation.YCoordinate) != null;

        public bool HasLocationToEast => CurrentWorld.LocationAt(CurrentLocation.XCoordinate + 1, CurrentLocation.YCoordinate) != null;

        public bool HasLocationToSouth => CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate - 1) != null;

        public bool HasMonster => CurrentMonster != null;

        public bool HasTrader => CurrentTrader != null;

        # endregion

        public GameSession()
        {
            CurrentPlayer = new Player
            {
                Name = "Chris",
                CharacterClass = "Fighter",
                CurrentHitPoints = 10,
                Gold = 1000000,
                Experience = 0,
                Level = 1
            };

            if (!CurrentPlayer.Weapons.Any())
            {
                CurrentPlayer.AddItemToInventory(ItemFactory.CreateGameItem(1001));
            }

            CurrentWorld = WorldFactory.CreateWorld();

            CurrentLocation = CurrentWorld.LocationAt(0, 0);
        }

        public void MoveNorth()
        {
            if (HasLocationToNorth)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate + 1);
            }
        }

        public void MoveWest()
        {
            if (HasLocationToWest)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate - 1, CurrentLocation.YCoordinate);
            }
        }

        public void MoveEast()
        {
            if (HasLocationToEast)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate + 1, CurrentLocation.YCoordinate);
            }
        }

        public void MoveSouth()
        {
            if (HasLocationToSouth)
            {
                CurrentLocation = CurrentWorld.LocationAt(CurrentLocation.XCoordinate, CurrentLocation.YCoordinate - 1);
            }
        }

        private void GivePlayerQuestsAtLocation ()
        {
            foreach (Quest quest in CurrentLocation.QuestsAvailableHere)
            {
                if (!CurrentPlayer.Quests.Any(q => q.PlayerQuest.ID == quest.ID))
                {
                    CurrentPlayer.Quests.Add(new QuestStatus(quest));
                    RaiseMessage("");
                    RaiseMessage($"You received the '{quest.Name}' quest");
                    RaiseMessage(quest.Description);

                    RaiseMessage("Return with:");
                    foreach (ItemQuantity item in quest.ItemsToComplete)
                    {
                        RaiseMessage($"- {item.Quantity} {ItemFactory.CreateGameItem(item.ItemID).Name}");
                    }

                    RaiseMessage("And you'll receive:");
                    RaiseMessage($"- {quest.RewardExperience} experience points");
                    RaiseMessage($"- {quest.RewardGold} gold");
                    foreach (ItemQuantity rewardItem in quest.RewardItems)
                    {
                        RaiseMessage($"- {rewardItem.Quantity} {ItemFactory.CreateGameItem(rewardItem.ItemID).Name}");
                    }
                }
            }
        }

        private void CompleteQuestsAtLocation ()
        {
            foreach (Quest quest in CurrentLocation.QuestsAvailableHere)
            {
                QuestStatus questToComplete = CurrentPlayer.Quests.FirstOrDefault(q => quest.ID == q.PlayerQuest.ID && !q.IsCompleted);
                if (questToComplete != null)
                {
                    if (CurrentPlayer.HasAllTheseItems(quest.ItemsToComplete))
                    {
                        // Remove the quest completion items from player's inventory
                        foreach (ItemQuantity item in quest.ItemsToComplete)
                        {
                            for (int n = 0; n < item.Quantity; n++)
                            {
                                CurrentPlayer.RemoveItemFromInventory(CurrentPlayer.Inventory.First(i => i.ItemTypeID == item.ItemID));
                            }
                        }

                        RaiseMessage("");
                        RaiseMessage($"You completed the {quest.Name} quest.");
                        RaiseMessage("You receive:");

                        CurrentPlayer.Experience += quest.RewardExperience;
                        RaiseMessage($"- {quest.RewardExperience} experience points");

                        CurrentPlayer.Gold += quest.RewardGold;
                        RaiseMessage($"- {quest.RewardGold} gold");

                        foreach (ItemQuantity rewardItem in quest.RewardItems)
                        {
                            GameItem questRewardItem = ItemFactory.CreateGameItem(rewardItem.ItemID);
                            CurrentPlayer.AddItemToInventory(questRewardItem);
                            RaiseMessage($"- {rewardItem.Quantity} {questRewardItem.Name}");
                        }

                        // Mark the quest as completed
                        questToComplete.IsCompleted = true;
                    }
                }
            }
        }

        private void GetMonsterAtLocation()
        {
            CurrentMonster = CurrentLocation.GetMonster();
        }

        public void AttackCurrentMonster()
        {
            if (CurrentWeapon == null)
            {
                RaiseMessage("You must select a weapon to attack.");
                return;
            }

            // Determine damage dealt to monster
            int damageDealt = RandomNumberGenerator.NumberBetween(CurrentWeapon.MinimumDamage, CurrentWeapon.MaximumDamage);

            if (damageDealt == 0)
            {
                RaiseMessage($"You miss the {CurrentMonster.Name}");
            }
            else
            {
                CurrentMonster.CurrentHitPoints -= damageDealt;
                RaiseMessage($"You hit the {CurrentMonster.Name} for {damageDealt} damage.");
            }

            // Player kills the monster
            if (CurrentMonster.CurrentHitPoints <= 0)
            {
                RaiseMessage("");
                RaiseMessage($"You defeat the {CurrentMonster.Name}.");

                CurrentPlayer.Experience += CurrentMonster.RewardExperience;
                RaiseMessage($"You receive {CurrentMonster.RewardExperience} experience points.");

                CurrentPlayer.Gold += CurrentMonster.Gold;
                RaiseMessage($"You receive {CurrentMonster.Gold} gold coins.");

                foreach (GameItem lootItem in CurrentMonster.Inventory)
                {
                    CurrentPlayer.AddItemToInventory(lootItem);
                    RaiseMessage($"You pick up {lootItem.Name}.");
                }

                // Get another monster to fight
                GetMonsterAtLocation();
            }
            else
            {
                // Determine damage dealt by monster's counterattack
                int damageReceived = RandomNumberGenerator.NumberBetween(CurrentMonster.MinimumDamage, CurrentMonster.MaximumDamage);
                if (damageReceived == 0)
                {
                    RaiseMessage($"The {CurrentMonster.Name} misses you.");
                }
                else
                {
                    CurrentPlayer.CurrentHitPoints -= damageReceived;
                    RaiseMessage($"The {CurrentMonster.Name} hits you for {damageReceived} damage.");
                }

                // The monster defeats the player
                if (CurrentPlayer.CurrentHitPoints <= 0)
                {
                    RaiseMessage("");
                    RaiseMessage($"The {CurrentMonster.Name} defeats you...");

                    CurrentLocation = CurrentWorld.LocationAt(0, -1); // Move the player to Player's Home
                    CurrentPlayer.CurrentHitPoints = CurrentPlayer.Level * 10; // Heal the player
                }
            }
        }

        private void RaiseMessage (string message)
        {
            OnMessageRaised?.Invoke(this, new GameMessageEventArgs(message));
        }
    }
}
