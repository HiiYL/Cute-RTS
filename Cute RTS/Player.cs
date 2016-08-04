using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cute_RTS
{
    class Player
    {
        public Color PlayerColor { get; set; }
        public List<BaseUnit> Units { get { return _units; } }
        public string Name { get; set; }
        public Player Opponent { get; set; }

        List<BaseUnit> _units;

        public Player(Color color, string name)
        {
            _units = new List<BaseUnit>();
            PlayerColor = color;
            Name = name;
        }

        public bool isMyUnit(BaseUnit bu)
        {
            return _units.Contains(bu);
        }

        public void addUnit(BaseUnit bu)
        {
            _units.Add(bu);
        }

        public bool removeUnit(BaseUnit bu)
        {
            return _units.Remove(bu);
        }
    }
}
