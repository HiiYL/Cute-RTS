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
            Idle,
            WalkUp,
            WalkDown,
            WalkRight,
            WalkLeft,
            AttackLeft
        }

        private Selectable selectable;
        private Sprite<Animation> _animation;
        private Animation animation = Animation.Idle;
        private PathMover pathmover;

        public BaseUnit(TextureAtlas atlas, TiledMap tmc, string collisionlayer)
        {
            selectable = new Selectable();
            _animation = new Sprite<Animation>();
            pathmover = new PathMover(tmc, collisionlayer, selectable);
            pathmover.OnDirectionChange += Pathmover_OnDirectionChange;
            pathmover.OnArrival += Pathmover_OnArrival;
            pathmover.OnCollision += Pathmover_OnCollision;
            pathmover.MoveSpeed = MoveSpeed;

            // Have path render below the unit
            pathmover.renderLayer = 1;

            setupAnimation(atlas);
            addComponent(selectable);
            addComponent(_animation);
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
            _animation.play(Animation.Idle);
        }

        private void Pathmover_OnDirectionChange(Vector2 moveDir)
        {
            
            if (Math.Abs(moveDir.X) > 5)
            {
                if (moveDir.X < 0)
                {
                    _animation.spriteEffects = SpriteEffects.None;
                    animation = Animation.WalkLeft;
                } else
                {
                    _animation.spriteEffects = SpriteEffects.FlipHorizontally;
                    animation = Animation.WalkRight;
                }

            } else if (Math.Abs(moveDir.Y) > 5)
            {
                if (moveDir.Y < 0)
                {
                    animation = Animation.WalkUp;
                } else if (moveDir.Y > 0)
                {
                    animation = Animation.WalkDown;
                }   
            }
            if (!_animation.isAnimationPlaying(animation))
            {
                Console.Write("Animation Changed, current : " + animation + "\n");
                _animation.play(animation);
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
            _animation.play(animation);

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
                _animation.color = Color.Green;
            } else
            {
                _animation.color = Color.White;
            }
        }

        private void setupAnimation(TextureAtlas atlas)
        {
            _animation.addAnimation(Animation.Idle, atlas.getSpriteAnimation("idle-down"));
            _animation.addAnimation(Animation.WalkDown, atlas.getSpriteAnimation("move-down"));
            _animation.addAnimation(Animation.WalkUp, atlas.getSpriteAnimation("move-up"));
            _animation.addAnimation(Animation.WalkLeft, atlas.getSpriteAnimation("move-front-left"));
            //TODO: Figure out how to flip X of animation
            _animation.spriteEffects = SpriteEffects.FlipHorizontally;

            _animation.addAnimation(Animation.WalkRight, atlas.getSpriteAnimation("move-front-left"));
            _animation.addAnimation(Animation.AttackLeft, atlas.getSpriteAnimation("attack-left"));
        }
    }
}
