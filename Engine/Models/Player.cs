using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Models
{
    class Player
    {
        string Name { get; set; }
        string CharacterClass { get; set; }
        int HitPoints { get; set; }
        int Experience { get; set; }
        int Level { get; set; }
        int Gold { get; set; }
    }
}
