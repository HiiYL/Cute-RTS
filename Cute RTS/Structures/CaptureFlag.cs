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

            _selectable = new Selectable();
            _selectTex = new Sprite(selectTex);
            _selectTex.enabled = false;
            _selectTex.color = Color.Yellow;
            _selectTex.renderLayer = 1;
            _selectTex.setLocalOffset(textureOffset);

            _collider = new CircleCollider(10);
            _collider.setLocalOffset(new Vector2(15, 15));
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)RTSCollisionLayer.Units);
            colliders.add(_collider);

            addComponent(_sprite);
            addComponent(_selectable);
            addComponent(_selectTex);
        }

        public override void onAddedToScene()
        {
            Selector.getSelector().OnSelectionChanged += new Selector.SelectionHandler(onChangeSelect);
            base.onAddedToScene();
        }

        private void onChangeSelect(IReadOnlyList<Selectable> sel)
        {
            if (_selectable.IsSelected)
            {
                _selectTex.enabled = true;
            }
            else
            {
                _selectTex.enabled = false;
            }
        }

        public void setSelectionColor(Color color)
        {
            _selectTex.color = color;
        }
    }
}
