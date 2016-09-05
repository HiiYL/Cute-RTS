using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cute_RTS
{
    class Player : Entity
    {
        public Color PlayerColor { get; set; }
        public List<Attackable> Units { get { return _units; } }
        public string Name { get; set; }
        public Player Opponent { get; set; }

        private List<Attackable> _units;

        public Player(Color color, string name)
        {
            _units = new List<Attackable>();
            PlayerColor = color;
            Name = name;
        }

        public bool isMyUnit(Attackable bu)
        {
            return _units.Contains(bu);
        }

        public void addUnit(Attackable bu)
        {
            _units.Add(bu);
        }

        public bool removeUnit(Attackable bu)
        {
            return _units.Remove(bu);
        }
    }
}
