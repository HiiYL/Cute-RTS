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
        public Player Opponent {
            get { return _tree.Opponent;  }
            set { _tree.Opponent = value; }
        }

        private PlayerBehaviourTree _tree;

        public PlayerAI(Color color, string name) : base(color, name)
        {
            _tree = new PlayerBehaviourTree();
            addComponent(_tree);
        }

        
    }
}
