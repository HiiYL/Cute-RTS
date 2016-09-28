using Cute_RTS.Structures;
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
        public MainBase mainBase { get; set; }
        public string Name { get; set; }
        public Player Opponent { get; set; }
        public int Gold {
            get { return _gold; }
            set
            {
                if (value < 0) throw new Exception("DUDE. No negative currency bruh!");
                if (value != _gold)
                {
                    _gold = value;
                    OnGoldChange?.Invoke(_gold);
                }
            }
        }

        public delegate void OnGoldChangeHandler(int amount);
        public event OnGoldChangeHandler OnGoldChange;

        private List<Attackable> _units;

        private int _gold = 50;

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
