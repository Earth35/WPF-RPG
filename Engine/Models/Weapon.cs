using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Models
{
    public class Weapon : GameItem
    {
        int MinimumDamage { get; set; }
        int MaximumDamage { get; set; }

        Weapon (int itemTypeID, string name, int price, int minimumDamage, int maximumDamage) : base (itemTypeID, name, price)
        {
            MinimumDamage = minimumDamage;
            MaximumDamage = maximumDamage;
        }
    }
}
