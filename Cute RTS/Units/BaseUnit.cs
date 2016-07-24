using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Units
{
    abstract class BaseUnit : Entity
    {
        Selectable selectable;
        Sprite sprite;

        public BaseUnit(Texture2D texture)
        {
            selectable = new Selectable();
            sprite = new Sprite(texture);

            addComponent(selectable);
            addComponent(sprite);
            colliders.add(new CircleCollider());
        }

        public override void onAddedToScene()
        {
            Selector.getSelector().OnSelectionChanged += new Selector.SelectionHandler(onChangeSelect);
            base.onAddedToScene();
        }

        public override void onRemovedFromScene()
        {
            Selector.getSelector().OnSelectionChanged -= onChangeSelect;
            base.onRemovedFromScene();
        }

        private void onChangeSelect(IReadOnlyList<Selectable> sel)
        {
            if (selectable.IsSelected)
            {
                sprite.color = Color.Green;
            } else
            {
                sprite.color = Color.White;
            }
        }
    }
}
