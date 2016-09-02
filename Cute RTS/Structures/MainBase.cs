using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Structures
{
    class MainBase : Attackable
    {
        private Selectable _selectable;
        private Sprite<Animation> _sprite;
        private BoxCollider _collider;

        public enum Animation
        {
            None,
            Default,
            BuildingUnit
        }

        public MainBase(TiledMap tmc, TextureAtlas atlas, Texture2D selectTex, Player player) :
            base(tmc)
        {
            UnitPlayer = player;
            _selectable = new Selectable(new Sprite(selectTex));
            _sprite = new Sprite<Animation>();
            _collider = new BoxCollider(-8, -8, 16, 16);

            OnUnitDied += MainBase_OnUnitDied;
        }

        private void MainBase_OnUnitDied(Attackable idied)
        {
            throw new NotImplementedException();
        }
    }
}
