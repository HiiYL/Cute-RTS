using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS
{
    class UnitRadar : RenderableComponent
    {
        public override float width { get { return 1000; } }
        public override float height { get { return 1000; } }
        
        public bool ShowArea { get; set; } = true;
        public int Radius { get; set; }

        private Player _player;
        private const int MAX_UNIT_RETURN = 100;
        private Collider[] _colliders = new Collider[MAX_UNIT_RETURN];

        public UnitRadar (int radius)
        {
            this.Radius = radius;
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            var bu = entity as BaseUnit;
            _player = bu.UnitPlayer;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            if (!ShowArea) return;

            graphics.batcher.drawCircle(
                entity.transform.position,
                Radius,
                Color.CadetBlue);
        }

        // finds the first enemy to step into your field of vision
        public Attackable detectEnemyInArea()
        {
            int count = Physics.overlapCircleAll(entity.transform.position, Radius, _colliders, layerMask: (int) RTSCollisionLayer.Map);

            for (int i = 0; i < count; i++)
            {
                Attackable bu = _colliders[i].entity as Attackable;
                if (bu == null || bu == entity || _player.isMyUnit(bu)) continue;
                return bu;
            }

            return null;
        }
    }
}
