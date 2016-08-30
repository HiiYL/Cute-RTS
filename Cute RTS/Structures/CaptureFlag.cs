using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Structures
{
    class CaptureFlag : Entity
    {
        private CircleCollider _collider;
        private Selectable _selectable;
        private Sprite _selectTex;
        private Sprite _sprite;

        public CaptureFlag(Texture2D tex, Texture2D selectTex)
        {
            var textureOffset = new Vector2(33, -5);

            _sprite = new Sprite(tex);
            // offset to set pitch point of flag as origin
            _sprite.setLocalOffset(textureOffset);
            var s = new Sprite(selectTex);
            s.setLocalOffset(textureOffset);
            _selectable = new Selectable(s);
            _selectable.setSelectionColor(Color.Yellow);

            _collider = new CircleCollider(10);
            _collider.setLocalOffset(new Vector2(15, 15));
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)RTSCollisionLayer.Units);
            colliders.add(_collider);

            addComponent(_sprite);
            addComponent(_selectable);
            //addComponent(_selectTex);
        }

        public override void onAddedToScene()
        {
            base.onAddedToScene();
        }

        public override void onRemovedFromScene()
        {
            base.onRemovedFromScene();
        }

    }
}
