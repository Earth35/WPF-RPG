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
            _standardGameItems.Add(new GameItem(9002, "Snake fang", 1));
            _standardGameItems.Add(new GameItem(9003, "Snakeskin", 2));
        }

        public static GameItem CreateGameItem(int itemID)
        {
            GameItem standardGameItem = _standardGameItems.FirstOrDefault(item => item.ItemTypeID == itemID);

            if (standardGameItem != null)
            {
                return (standardGameItem.Clone());
            }

            return null;            
        }
    }
}
