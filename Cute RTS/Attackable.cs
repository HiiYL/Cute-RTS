using Microsoft.Xna.Framework;
using Nez;
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
                    if (!isAlive)
                    {
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
        public bool isAlive { get { return _currenthealth > 0; } }
        public delegate void OnUnitDiedHandler(Attackable idied);
        public event OnUnitDiedHandler OnUnitDied;
        public delegate void OnHealthChangeHandler(float healthpercentage);
        public event OnHealthChangeHandler OnHealthChange;

        private int _currenthealth;
        private int _fullhealth;
        private TiledMap _tilemap;

        public Attackable(TiledMap tmc)
        {
            // start with full health
            _currenthealth = _fullhealth = 10;
            _tilemap = tmc;
        }

        public virtual float HealthPercentage
        {
            get { return _currenthealth / (float)FullHealth; }
        }

        public Point getTilePosition()
        {
            return _tilemap.worldToTilePosition(transform.position);
        }
    }
}
