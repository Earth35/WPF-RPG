using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Models;

namespace Engine.Factories
{
    internal static class ItemFactory
    {
        private static List<GameItem> _standardGameItems;

        static ItemFactory ()
        {
            _standardGameItems = new List<GameItem>();

            _standardGameItems.Add(new Weapon(1001, "Pointy stick", 1, 0, 2));
            _standardGameItems.Add(new Weapon(1002, "Rusty sword", 5, 1, 3));
            _standardGameItems.Add(new GameItem(9001, "Snake fang", 1));
            _standardGameItems.Add(new GameItem(9002, "Snakeskin", 2));
            _standardGameItems.Add(new GameItem(9003, "Rat tail", 1));
            _standardGameItems.Add(new GameItem(9004, "Rat fur", 2));
            _standardGameItems.Add(new GameItem(9005, "Spider fang", 3));
            _standardGameItems.Add(new GameItem(9006, "Spider silk", 5));
        }

        public static GameItem CreateGameItem(int itemID)
        {
            GameItem standardGameItem = _standardGameItems.FirstOrDefault(item => item.ItemTypeID == itemID);

            if (standardGameItem != null)
            {
                if (standardGameItem is Weapon)
                {
                    return (standardGameItem as Weapon).Clone();
                }

                return (standardGameItem.Clone());
            }

            return null;            
        }
    }
}
