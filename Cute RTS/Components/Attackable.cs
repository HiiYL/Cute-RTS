using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS
{
    class Attackable : Entity
    {
        public int CurrentHealth
        {
            get { return _currenthealth; }
            set
            {
                int newHealth = value;

                // health cannot go pass limit
                if (newHealth > FullHealth)
                {
                    newHealth = FullHealth;
                }

                if (_currenthealth != newHealth)
                {
                    _currenthealth = newHealth;
                    OnHealthChange?.Invoke(HealthPercentage);
                    if (isAlive && _currenthealth <= 0) // you only die once
                    {
                        isAlive = false; 
                        OnUnitDied?.Invoke(this);
                    }
                }
            }
        }
        public int FullHealth
        {
            get { return _fullhealth; }
            // match ratio of current health with new total health
            set {
                var percen = HealthPercentage;
                _fullhealth = value;
                _currenthealth = (int)(_fullhealth * percen);
            }
        }
        public bool isAlive { get; set; } = true;
        public delegate void OnUnitDiedHandler(Attackable idied);
        public event OnUnitDiedHandler OnUnitDied;
        public delegate void OnHealthChangeHandler(float healthpercentage);
        public event OnHealthChangeHandler OnHealthChange;
        public Player UnitPlayer { get { return _player; } }
        public TiledMap UnitTileMap { get { return _tilemap; }  }
        public Selectable Select { get { return _selectable; } }

        private Player _player;
        private int _currenthealth;
        private int _fullhealth;
        private TiledMap _tilemap;
        protected Selectable _selectable;

        public Attackable(TiledMap tmc, Sprite selectTex, Player player)
        {
            _player = player;

            // start with full health
            _currenthealth = _fullhealth = 10;
            _tilemap = tmc;
            _selectable = new Selectable(selectTex);
            addComponent(_selectable);
        }

        public virtual float HealthPercentage
        {
            get { return _currenthealth / (float)FullHealth; }
        }

        public Point getTilePosition()
        {
            return _tilemap.worldToTilePosition(transform.position);
        }

        public override void onAddedToScene()
        {
            base.onAddedToScene();
            UnitPlayer.addUnit(this);
        }

        public override void onRemovedFromScene()
        {
            base.onRemovedFromScene();
            UnitPlayer.removeUnit(this);
        }
    }
}
