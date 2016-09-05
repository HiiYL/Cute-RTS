using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Cute_RTS
{
    class AIPlayer : Player
    {
        public AIPlayer(Color color, string name, Player opponent) : base(color, name)
        {
            Opponent = opponent;
            //addComponent(new PlayerBehaviourTree(this, opponent));
        }

        public enum UnitCommand
        {
            None,
            Idle,
            GoTo,
            Follow,
            AttackUnit,
            AttackLocation,
            CaptureFlag
        }
        public UnitCommand ActiveCommand { get; set; } = UnitCommand.Idle;
    }
}
