using Microsoft.Xna.Framework;
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

namespace Cute_RTS.Units
{
    abstract class BaseUnit : Entity
    {
        // Unit Properties:
        public virtual int Health { get; set; } = 10;
        public virtual int Damage { get; set; } = 10;
        public virtual int Range { get; set; } = 1; // default is melee
        public virtual int Vision { get; set; } = 8;
        public virtual int MoveSpeed { get; set; } = 15;

        public enum Animation
        {
            None,
            Idle,
            WalkUp,
            WalkDown,
            WalkRight,
            WalkLeft,
            AttackLeft
        }

        private Selectable selectable;
        private Sprite<Animation> sprite;
        private Animation animation = Animation.Idle;
        private PathMover pathmover;

        public BaseUnit(TextureAtlas atlas, TiledMap tmc, string collisionlayer)
        {
            selectable = new Selectable();
            sprite = new Sprite<Animation>();
            pathmover = new PathMover(tmc, collisionlayer, selectable);
            pathmover.OnDirectionChange += Pathmover_OnDirectionChange;
            pathmover.OnArrival += Pathmover_OnArrival;
            pathmover.OnCollision += Pathmover_OnCollision;
            pathmover.MoveSpeed = MoveSpeed;

            // Have path render below the unit
            pathmover.renderLayer = 1;

            setupAnimation(atlas);
            addComponent(selectable);
            addComponent(sprite);
            addComponent(pathmover);
            colliders.add(new CircleCollider());
        }

        private void Pathmover_OnCollision(ref CollisionResult res)
        {
            // When unit collides with anything, it simply gives up.
            //pathmover.stopMoving();
            //sprite.play(Animation.Idle);
        }

        private void Pathmover_OnArrival()
        {
            sprite.play(Animation.Idle);
        }

        private void Pathmover_OnDirectionChange(Vector2 moveDir)
        {
            Animation newAnim = Animation.None;

            if (moveDir.X == -1)
            {
                sprite.spriteEffects = SpriteEffects.None;
                newAnim = Animation.WalkLeft;
            } else if (moveDir.X == 1)
            {
                sprite.spriteEffects = SpriteEffects.FlipHorizontally;
                newAnim = Animation.WalkRight;
            } else if (moveDir.Y == -1)
            {
                newAnim = Animation.WalkUp;
            } else if (moveDir.Y == 1)
            {
                newAnim = Animation.WalkDown;
            }   


            if (newAnim != Animation.None)
            {
                animation = newAnim;
                sprite.play(animation);
            }
            
        }

        /// <summary>
        /// Unit will move to target location according to its move speed.
        /// </summary>
        /// <returns>false if path does not exist, true otherwise.</returns>
        public bool gotoLocation(Vector2 target)
        {
            return false;
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
            sprite.addAnimation(Animation.Idle, atlas.getSpriteAnimation("idle-down"));
            sprite.addAnimation(Animation.WalkDown, atlas.getSpriteAnimation("move-down"));
            sprite.addAnimation(Animation.WalkUp, atlas.getSpriteAnimation("move-up"));
            sprite.addAnimation(Animation.WalkLeft, atlas.getSpriteAnimation("move-front-left"));
            //TODO: Figure out how to flip X of animation
            sprite.spriteEffects = SpriteEffects.FlipHorizontally;
          
            sprite.addAnimation(Animation.WalkRight, atlas.getSpriteAnimation("move-front-left"));
            sprite.addAnimation(Animation.AttackLeft, atlas.getSpriteAnimation("attack-left"));
        }
    }
}
