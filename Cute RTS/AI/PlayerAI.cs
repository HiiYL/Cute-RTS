using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Cute_RTS.AI
{
    class PlayerAI : Player
    {
        public PlayerAI(Color color, string name, Player enemy) : base(color, name)
        {
            addComponent(new PlayerBehaviourTree(enemy));
        }

        
    }
}
