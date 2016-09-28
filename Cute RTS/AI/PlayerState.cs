using Cute_RTS.Structures;
using Cute_RTS.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.AI
{
    class PlayerState
    {
        Player _player;
        MainBase _mainbase;
        public List<Attackable> Threats { get; set; }
        public List<BaseUnit> Defenders { get; set; }

        public PlayerState(Player player)
        {
            _player = player;
            _mainbase = player.mainBase;
        }

        public void updateState()
        {
            Threats = _mainbase.getSurroundingEnemies();
            Defenders = new List<BaseUnit>();

            foreach (var att in _player.Units)
            {
                if (!(att is BaseUnit)) continue;

                var bu = att as BaseUnit;
                if (bu.ActiveCommand == BaseUnit.UnitCommand.AttackLocation 
                    && bu.AttackLocation == _mainbase.transform.position.ToPoint())
                {
                    Defenders.Add(bu);
                }
            }
        }

        public int getThreatLevel()
        {
            return Threats.Count - Defenders.Count;
        }
    }
}
