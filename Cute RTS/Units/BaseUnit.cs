using Cute_RTS.Scenes;
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
    class BaseUnit : Entity
    {
        // Unit Properties:
        public virtual int Health
        {
            get { return _health; }
            set
            {
                if (value <= 0)
                {
                    OnUnitDied?.Invoke(this);
                    die();
                }
                else
                {
                    _health = value;
                }
            }
        }
        public virtual int FullHealth
        {
            get { return _fullhealth; }
        }
        public virtual float GetHealthPercentage
        {
            get { return _health / (float)_fullhealth; }
        }

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
                pathmover.MoveSpeed = _moveSpeed;
            }
        }
        public virtual float AttackSpeed { get; set; } = 1.5f; // in seconds
        public Point TargetLocation { get; set; }
        public BaseUnit TargetUnit { get; set; }
        public UnitCommand ActiveCommand { get; set; } = UnitCommand.Idle;
        public Player UnitPlayer { get { return _player; } }
        public UnitRadar Radar { get { return _radar; } }
        public delegate void OnUnitDiedHandler(BaseUnit idied);
        public event OnUnitDiedHandler OnUnitDied;

        private int _health;
        private int _fullhealth;
        private Player _player;
        private Animation animation;
        private TiledMap _tilemap;
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
            Attack
        }

        // components
        private PathMover pathmover;
        private BoxCollider collider;
        private Selectable selectable;
        private Sprite<Animation> sprite;
        private Sprite _selectTex;
        public ProgressBar healthBar;

        public BaseUnit(TextureAtlas atlas, Texture2D selectTex, Player player, TiledMap tmc, string collisionlayer)
        {
            _health = 40;
            _fullhealth = _health;
            _tilemap = tmc;
            _selectTex = new Sprite(selectTex);
            _selectTex.enabled = false;
            selectable = new Selectable();
            sprite = new Sprite<Animation>();
            pathmover = new PathMover(tmc, collisionlayer, selectable);
            collider = new BoxCollider(-8, -8, 16, 16);
            pathmover.OnDirectionChange += Pathmover_OnDirectionChange;
            pathmover.OnArrival += Pathmover_OnArrival;
            pathmover.OnCollision += Pathmover_OnCollision;
            pathmover.MoveSpeed = MoveSpeed;
            _player = player;
            _player.addUnit(this);
            sprite.color = _player.PlayerColor;
            _radar = new UnitRadar(Vision * 10);

            Flags.setFlagExclusive(ref collider.physicsLayer, (int)RTSCollisionLayer.Units);
            transform.setScale(new Vector2(0.5f, 0.5f));

            // Have path render below the unit
            pathmover.renderLayer = 1;
            _selectTex.renderLayer = 1;

            setupAnimation(atlas);
            addComponent(selectable);
            addComponent(_selectTex);
            addComponent(sprite);
            addComponent(pathmover);
            addComponent(_radar);
            addComponent(new UnitBehaviorTree(this, pathmover));
            colliders.add(collider);

        }

        internal void attackUnit(BaseUnit g)
        {
            ActiveCommand = UnitCommand.Attack;
            TargetUnit = g;
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
            }
            else if (moveDir.X == 1)
            {
                sprite.spriteEffects = SpriteEffects.FlipHorizontally;
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
            healthBar = ((BaseScene)scene).canvas.stage.addElement(new ProgressBar(0, 1, 0.1f, false, ProgressBarStyle.create(Color.Black, Color.White)));
            healthBar.setWidth(64);


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
                _selectTex.enabled = true;
            }
            else
            {
                _selectTex.enabled = false;
            }
        }

        public void playClickSelectAnimation(int counter = 4)
        {
            if (counter == 0) return;

            _selectTex.enabled = !_selectTex.enabled;
            Core.schedule(0.15f, tmr =>
            {
                playClickSelectAnimation(counter - 1);
            });
        }

        private void setupAnimation(TextureAtlas atlas)
        {
            var deathAnim = atlas.getSpriteAnimation("death-left");
            var attackFront = atlas.getSpriteAnimation("attack-front-left");
            var attackBack = atlas.getSpriteAnimation("attack-back-left");
            deathAnim.loop = false;
            //attackFront.loop = false;
            //attackBack.loop = false;

            sprite.addAnimation(Animation.Idle, atlas.getSpriteAnimation("idle-front-left"));
            sprite.addAnimation(Animation.WalkDown, atlas.getSpriteAnimation("move-down"));
            sprite.addAnimation(Animation.WalkUp, atlas.getSpriteAnimation("move-up"));
            sprite.addAnimation(Animation.WalkLeft, atlas.getSpriteAnimation("move-front-left"));
            sprite.addAnimation(Animation.WalkRight, atlas.getSpriteAnimation("move-front-left"));
            sprite.addAnimation(Animation.AttackFront, attackFront);
            sprite.addAnimation(Animation.AttackBack, attackBack);
            sprite.addAnimation(Animation.Die, deathAnim);
        }

        private void die()
        {
            if (_deathTimer == true) return;

            _deathTimer = true;
            pathmover.stopMoving();
            ActiveCommand = UnitCommand.None;
            playAnimation(Animation.Die);
            Core.schedule(1.5f, timer =>
            {
                UnitPlayer.removeUnit(this);
                destroy();
                healthBar.setIsVisible(false); //TODO: FIGURE OUT TO REMOVE IT COMPLETELY

            });
        }

        // execute attack, asumming target is already in range
        public void executeAttack()
        {
            if (TargetUnit == null) return;

            Console.WriteLine("ATTACK ENEMY! Health on target: " + TargetUnit.Health.ToString());

            Vector2 diff = transform.position - TargetUnit.transform.position;

            if (diff.X < 0)
            {
                sprite.spriteEffects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                sprite.spriteEffects = SpriteEffects.None;
            }

            if (diff.Y < 0)
            {
                playAnimation(Animation.AttackFront);
            }
            else
            {
                playAnimation(Animation.AttackBack);
            }

            bool killedTarget = Damage >= TargetUnit.Health;
            TargetUnit.Health -= Damage;

            if (killedTarget)
            {
                ActiveCommand = UnitCommand.Idle;
                TargetUnit = null;
            }
        }

        public void setSelectionColor(Color color)
        {
            _selectTex.color = color;
        }
    }
}
