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

        public enum UnitCommand
        {
            None,
            Idle,
            GoTo,
            Follow
        }

        public Point TargetLocation { get; set; }
        public BaseUnit TargetUnit { get; set; }
        public UnitCommand ActiveCommand { get; set; }
        private Animation animation;
        private TiledMap _tilemap;

        // components
        private PathMover pathmover;
        private BoxCollider collider;
        private Selectable selectable;
        private Sprite<Animation> sprite;

        public BaseUnit(TextureAtlas atlas, TiledMap tmc, string collisionlayer)
        {
            _tilemap = tmc;
            selectable = new Selectable();
            sprite = new Sprite<Animation>();
            pathmover = new PathMover(tmc, collisionlayer, selectable);
            collider = new BoxCollider(-8, -8, 16, 16);
            pathmover.OnDirectionChange += Pathmover_OnDirectionChange;
            pathmover.OnArrival += Pathmover_OnArrival;
            pathmover.OnCollision += Pathmover_OnCollision;
            pathmover.MoveSpeed = MoveSpeed;

            Flags.setFlagExclusive(ref collider.physicsLayer, (int) RTSCollisionLayer.Units);

            // Have path render below the unit
            pathmover.renderLayer = 1;

            setupAnimation(atlas);
            addComponent(selectable);
            addComponent(sprite);
            addComponent(pathmover);
            addComponent(new UnitBehaviorTree(this, pathmover));
            colliders.add(collider);
        }

        public Point getTilePosition()
        {
            return _tilemap.worldToTilePosition(transform.position);
        }

        private void Pathmover_OnCollision(ref CollisionResult res)
        {
            // When unit collides with anything, it simply gives up.
            //pathmover.stopMoving();
            //sprite.play(Animation.Idle);
        }

        private void Pathmover_OnArrival()
        {
            //playAnimation(Animation.Idle);
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

            playAnimation(newAnim);
        }

        public void playAnimation(Animation anim)
        {
            if (anim != Animation.None && animation != anim)
            {
                animation = anim;
                sprite.play(animation);
            }
        }

        /// <summary>
        /// Unit will move to target location according to its move speed.
        /// </summary>
        /// <returns>false if path does not exist, true otherwise.</returns>
        public bool gotoLocation(Point target)
        {
            bool canGoTo = pathmover.setTargetLocation(target);

            if (canGoTo) ActiveCommand = UnitCommand.GoTo;

            return canGoTo;
        }

        public void stopMoving()
        {
            ActiveCommand = UnitCommand.Idle;
        }

        public void followUnit(BaseUnit bu)
        {
            TargetUnit = bu;
            ActiveCommand = UnitCommand.Follow;
        }

        public override void onAddedToScene()
        {
            playAnimation(Animation.Idle);

            Selector.getSelector().OnSelectionChanged += new Selector.SelectionHandler(onChangeSelect);
            base.onAddedToScene();
        }

        public override void onRemovedFromScene()
        {
            Selector.getSelector().OnSelectionChanged -= onChangeSelect;
            base.onRemovedFromScene();
        }

        public void setPosition(Vector2 point)
        {
            transform.position = point;
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
            sprite.addAnimation(Animation.Idle, atlas.getSpriteAnimation("idle-front-left"));
            sprite.addAnimation(Animation.WalkDown, atlas.getSpriteAnimation("move-down"));
            sprite.addAnimation(Animation.WalkUp, atlas.getSpriteAnimation("move-up"));
            sprite.addAnimation(Animation.WalkLeft, atlas.getSpriteAnimation("move-front-left"));
            sprite.addAnimation(Animation.WalkRight, atlas.getSpriteAnimation("move-front-left"));
            sprite.addAnimation(Animation.AttackLeft, atlas.getSpriteAnimation("attack-front-left"));
        }
    }
}
