using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Units
{
    abstract class BaseUnit : Entity
    {
        // Unit Properties:
        public int Health { get; set; } = 10;
        public int Damage { get; set; } = 10;
        public int Range { get; set; } = 1; // default is melee
        public int Vision { get; set; } = 8;

        public enum Animations
        {
            Idle,
            WalkUp,
            WalkDown,
            WalkRight,
            WalkLeft,
            AttackLeft
        }

        Selectable selectable;
        Mover _mover;
        Sprite<Animations> sprite;
        Animations animation = Animations.Idle;

        public BaseUnit(TextureAtlas atlas)
        {
            selectable = new Selectable();
            sprite = new Sprite<Animations>();
            _mover = new Mover();

            setupAnimation(atlas);

            addComponent(selectable);
            addComponent(sprite);
            addComponent(_mover);
            colliders.add(new CircleCollider());
        }

        public override void onAddedToScene()
        {
            sprite.play(animation);

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

        private void setupAnimation(TextureAtlas atlas)
        {
            sprite.addAnimation(Animations.Idle, atlas.getSpriteAnimation("idle-down"));
            sprite.addAnimation(Animations.WalkDown, atlas.getSpriteAnimation("move-down"));
            sprite.addAnimation(Animations.WalkUp, atlas.getSpriteAnimation("move-up"));
            sprite.addAnimation(Animations.WalkLeft, atlas.getSpriteAnimation("move-front-left"));
            //TODO: Figure out how to flip X of animation
            sprite.addAnimation(Animations.WalkRight, atlas.getSpriteAnimation("move-front-left"));
            sprite.addAnimation(Animations.AttackLeft, atlas.getSpriteAnimation("attack-left"));
        }
    }
}
