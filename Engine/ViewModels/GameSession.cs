using Engine.EventArgs;
using Engine.Factories;
using Engine.Models;
using System;
using System.Linq;

namespace Engine.ViewModels
{
    public class GameSession : BaseNotificationClass
    {
        public event EventHandler<GameMessageEventArgs> OnMessageRaised;

        private Player _currentPlayer;
        private Location _currentLocation;
        private Monster _currentMonster;
        private Trader _currentTrader;

        #region Properties

        public Player CurrentPlayer
        {
            get { return _currentPlayer; }
            set
            {
                if (_currentPlayer != null)
                {
                    _currentPlayer.OnLeveledUp -= OnCurrentPlayerLeveledUp;
                    _currentPlayer.OnKilled -= OnCurrentPlayerKilled;
                }

                _currentPlayer = value;

                if (_currentPlayer != null)
                {
                    _currentPlayer.OnLeveledUp += OnCurrentPlayerLeveledUp;
                    _currentPlayer.OnKilled += OnCurrentPlayerKilled;
                }
            }
        }
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
                if (_currentMonster != null)
                {
                    _currentMonster.OnKilled -= OnCurrentMonsterKilled;
                }

                _currentMonster = value;

                if (CurrentMonster != null)
                {
                    _currentMonster.OnKilled += OnCurrentMonsterKilled;

                    RaiseMessage("");
                    RaiseMessage($"You see a {CurrentMonster.Name} here!");
                }

                OnPropertyChanged(nameof(CurrentMonster));
                OnPropertyChanged(nameof(HasMonster));
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
            CurrentPlayer = new Player("Chris", "Fighter", 0, 10, 10, 1000000);

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

                        RaiseMessage($"- {quest.RewardExperience} experience points");
                        CurrentPlayer.GainExperience(quest.RewardExperience);
                                                
                        RaiseMessage($"- {quest.RewardGold} gold");
                        CurrentPlayer.ReceiveGold(quest.RewardGold);

                        foreach (ItemQuantity rewardItem in quest.RewardItems)
                        {
                            GameItem questRewardItem = ItemFactory.CreateGameItem(rewardItem.ItemID);
                            RaiseMessage($"- {rewardItem.Quantity} {questRewardItem.Name}");
                            CurrentPlayer.AddItemToInventory(questRewardItem);
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
                RaiseMessage($"You hit the {CurrentMonster.Name} for {damageDealt} damage.");
                CurrentMonster.TakeDamage(damageDealt);
            }

            // Player kills the monster
            if (CurrentMonster.IsDead)
            {
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
                    RaiseMessage($"The {CurrentMonster.Name} hits you for {damageReceived} damage.");
                    CurrentPlayer.TakeDamage(damageReceived);
                }
            }
        }

        private void OnCurrentPlayerLeveledUp(object sender, System.EventArgs eventArgs)
        {
            RaiseMessage($"You advanced to level {CurrentPlayer.Level}.");
        }

        private void OnCurrentPlayerKilled(object sender, System.EventArgs eventArgs)
        {
            RaiseMessage("");
            RaiseMessage($"The {CurrentMonster.Name} killed you.");

            CurrentLocation = CurrentWorld.LocationAt(0, -1);
            CurrentPlayer.CompletelyHeal();
        }

        private void OnCurrentMonsterKilled(object sender, System.EventArgs eventArgs)
        {
            RaiseMessage("");
            RaiseMessage($"You defeated the {CurrentMonster.Name}!");

            RaiseMessage($"You receive {CurrentMonster.RewardExperience} experience.");
            CurrentPlayer.GainExperience(CurrentMonster.RewardExperience);

            RaiseMessage($"You receive {CurrentMonster.Gold} gold.");
            CurrentPlayer.ReceiveGold(CurrentMonster.Gold);

            foreach (GameItem gameItem in CurrentMonster.Inventory)
            {
                RaiseMessage($"You receive one {gameItem.Name}.");
                CurrentPlayer.AddItemToInventory(gameItem);
            }
        }

        private void RaiseMessage (string message)
        {
            OnMessageRaised?.Invoke(this, new GameMessageEventArgs(message));
        }
    }
}
