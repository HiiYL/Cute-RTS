﻿using Cute_RTS.Components;
using Cute_RTS.Scenes;
using Cute_RTS.Structures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Tiled;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Units
{
    class BaseUnit : Attackable
    {
        // Unit Properties:
        public virtual int Damage { get; set; } = 10;
        public virtual float Range { get; set; } = 1.5f; // default is melee
        public virtual int Vision
        {
            get { return _vision; }
            set
            {
                _vision = value;
                _radar.Radius = _vision;
            }
        }
        public virtual int MoveSpeed
        {
            get { return _moveSpeed; }
            set
            {
                _moveSpeed = value;
                _pathmover.MoveSpeed = _moveSpeed;
            }
        }
        public virtual float AttackSpeed { get; set; } = 1.5f; // in seconds
        public Point TargetLocation { get; set; }
        public Attackable TargetUnit { get; set; }
        public UnitCommand ActiveCommand { get; set; } = UnitCommand.Idle;
        public UnitRadar Radar { get { return _radar; } }
        
        public Point AttackLocation { get; set; }
        public CaptureFlag TargetFlag { get; set; } = null;
        
        private Animation _animation;
        private bool _deathTimer = false;
        private UnitRadar _radar;
        private int _moveSpeed = 10;
        private int _vision = 9;

        public enum Animation
        {
            None,
            Idle,
            WalkUp,
            WalkDown,
            WalkRight,
            WalkLeft,
            AttackFront,
            Die,
            AttackBack
        }

        public enum UnitCommand
        {
            None,
            Idle,
            GoTo,
            Follow,
            AttackUnit,
            AttackLocation,
            CaptureFlag
        }

        // components
        private PathMover _pathmover;
        private BoxCollider _collider;
        private Sprite<Animation> _sprite;

        public BaseUnit(TextureAtlas atlas, Texture2D selectTex, Player player, TiledMap tmc, string collisionlayer) :
            base(tmc, new Sprite(selectTex), player)
        {
            FullHealth = 40;

            OnUnitDied += BaseUnit_OnUnitDied;
            
            _sprite = new Sprite<Animation>();
            _pathmover = new PathMover(tmc, collisionlayer, _selectable);
            _collider = new BoxCollider(-8, -8, 16, 16);
            _pathmover.OnDirectionChange += Pathmover_OnDirectionChange;
            _pathmover.MoveSpeed = MoveSpeed;
            _sprite.color = UnitPlayer.PlayerColor;
            _radar = new UnitRadar(Vision * 10);

            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)RTSCollisionLayer.Units);
            transform.setScale(new Vector2(0.5f, 0.5f));

            // Have path render below the unit
            _pathmover.renderLayer = 1;

            setupAnimation(atlas);
            var h = new HealthBar(this);
            h.PositionOffset = new Vector2(32, 25);
            addComponent(h);
            addComponent(_sprite);
            addComponent(_pathmover);
            addComponent(_radar);
            addComponent(new UnitBehaviorTree(this, _pathmover));
            colliders.add(_collider);
        }

        private void BaseUnit_OnUnitDied(Attackable idied)
        {
            if (_deathTimer == true) return;

            _deathTimer = true;
            _pathmover.stopMoving();
            ActiveCommand = UnitCommand.None;
            playAnimation(Animation.Die);
            Core.schedule(1.5f, timer =>
            {
                destroy();
            });
        }

        public bool attackLocation(Point target)
        {
            bool canGoTo = _pathmover.setTargetLocation(target);

            if (canGoTo)
            {
                AttackLocation = target;
                ActiveCommand = UnitCommand.AttackLocation;
            }

            return canGoTo;
        }

        public void attackUnit(Attackable g)
        {
            // stop going anywhere; time to kill this son of a bitch!
            _pathmover.stopMoving();
            ActiveCommand = UnitCommand.AttackUnit;
            TargetUnit = g;
        }

        private void Pathmover_OnDirectionChange(Vector2 moveDir)
        {
            Animation newAnim = Animation.None;

            if (moveDir.X == -1)
            {
                _sprite.spriteEffects = SpriteEffects.None;
                newAnim = Animation.WalkLeft;
            }
            else if (moveDir.X == 1)
            {
                _sprite.spriteEffects = SpriteEffects.FlipHorizontally;
                newAnim = Animation.WalkRight;
            }
            else if (moveDir.Y == -1)
            {
                newAnim = Animation.WalkUp;
            }
            else if (moveDir.Y == 1)
            {
                newAnim = Animation.WalkDown;
            }

            playAnimation(newAnim);
        }

        public void playAnimation(Animation anim)
        {
            if (anim != Animation.None && _animation != anim)
            {
                _animation = anim;
                _sprite.play(_animation);
            }
        }

        /// <summary>
        /// Unit will move to target location according to its move speed.
        /// </summary>
        /// <returns>false if path does not exist, true otherwise.</returns>
        public bool gotoLocation(Point target)
        {
            bool canGoTo = _pathmover.setTargetLocation(target);

            if (canGoTo) ActiveCommand = UnitCommand.GoTo;

            return canGoTo;
        }

        public void stopMoving()
        {
            _pathmover.stopMoving();
            ActiveCommand = UnitCommand.Idle;
        }

        public void followUnit(Attackable bu)
        {
            TargetUnit = bu;
            ActiveCommand = UnitCommand.Follow;
        }

        public bool captureFlag(CaptureFlag flag)
        {
            Point p = flag.getPosition().ToPoint();
            bool r = _pathmover.setTargetLocation(p);
            if (r == false) return r;

            TargetFlag = flag;
            ActiveCommand = UnitCommand.CaptureFlag;

            return true;
        }

        public override void onAddedToScene()
        {
            playAnimation(Animation.Idle);
            base.onAddedToScene();
        }

        public void setPosition(Vector2 point)
        {
            transform.position = point;
        }


        private void setupAnimation(TextureAtlas atlas)
        {
            var deathAnim = atlas.getSpriteAnimation("death-left");
            var attackFront = atlas.getSpriteAnimation("attack-front-left");
            var attackBack = atlas.getSpriteAnimation("attack-back-left");
            deathAnim.loop = false;
            //attackFront.loop = false;
            //attackBack.loop = false;

            _sprite.addAnimation(Animation.Idle, atlas.getSpriteAnimation("idle-front-left"));
            _sprite.addAnimation(Animation.WalkDown, atlas.getSpriteAnimation("move-down"));
            _sprite.addAnimation(Animation.WalkUp, atlas.getSpriteAnimation("move-up"));
            _sprite.addAnimation(Animation.WalkLeft, atlas.getSpriteAnimation("move-front-left"));
            _sprite.addAnimation(Animation.WalkRight, atlas.getSpriteAnimation("move-front-left"));
            _sprite.addAnimation(Animation.AttackFront, attackFront);
            _sprite.addAnimation(Animation.AttackBack, attackBack);
            _sprite.addAnimation(Animation.Die, deathAnim);
        }

        // execute attack, asumming target is already in range
        public void executeAttack()
        {
            if (TargetUnit == null) return;

            Console.WriteLine("ATTACK ENEMY! Health on target: " + TargetUnit.CurrentHealth.ToString());

            Vector2 diff = transform.position - TargetUnit.transform.position;

            if (diff.X < 0)
            {
                _sprite.spriteEffects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _sprite.spriteEffects = SpriteEffects.None;
            }

            if (diff.Y < 0)
            {
                playAnimation(Animation.AttackFront);
            }
            else
            {
                playAnimation(Animation.AttackBack);
            }

            bool killedTarget = Damage >= TargetUnit.CurrentHealth;
            TargetUnit.CurrentHealth -= Damage;

            if (killedTarget)
            {
                ActiveCommand = UnitCommand.Idle;
                TargetUnit = null;
            }
        }

    }
}
