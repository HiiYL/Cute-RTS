using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Sprites;
using Nez.Systems;
using Nez.TextureAtlases;
using Nez.Tiled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Units
{
    class HumanFootman : RenderableComponent, ITriggerListener, IUpdatable
    {

        public override float width { get { return 1000; } }
        public override float height { get { return 1000; } }

        public enum Animations
        {
            Idle,
            WalkUp,
            WalkDown,
            WalkRight,
            WalkLeft,
            AttackLeft
        }
        Sprite<Animations> _animation;
        Animations animation = Animations.Idle;
        Mover _mover;
        float _moveSpeed = 100f;
        Vector2 _projectileVelocity = new Vector2(175);




        VirtualButton _fireInput;
        VirtualIntegerAxis _xAxisInput;
        VirtualIntegerAxis _yAxisInput;
        private TiledMap _tilemap;

        public bool interactable = true;

        WeightedGridGraph _astarGraph;
        List<Point> _astarSearchPath;

        bool isDone = false;

        int current_node = 0;

        Point _start, _end;
        private Vector2 moveDir;

        public HumanFootman(TiledMap tilemap, bool interactable = true)
        {
            this.interactable = interactable;
            _tilemap = tilemap;
            var layer = tilemap.getLayer<TiledTileLayer>("Stuff");

            _start = new Point(1, 1);
            _end = new Point(10, 10);

            _astarGraph = new WeightedGridGraph(layer);
            _astarSearchPath = _astarGraph.search(_start, _end);

            //Debug.drawTextFromBottom = true;
        }

        public override void onAddedToEntity()
        {

            var atlas = entity.scene.content.Load<TextureAtlas>("BaldyAtlas");
            var anim = atlas.getSpriteAnimation("idle-down");

            _animation = new Sprite<Animations>(Animations.Idle, anim);


            _mover = entity.addComponent(new Mover());
            var shadow = entity.addComponent(new SpriteMime(_animation));
            shadow.color = new Color(10, 10, 10, 80);
            shadow.material = Material.stencilRead();
            shadow.renderLayer = -2;

            _animation.addAnimation(Animations.WalkDown, atlas.getSpriteAnimation("move-down"));
            _animation.addAnimation(Animations.WalkUp, atlas.getSpriteAnimation("move-up"));
            _animation.addAnimation(Animations.WalkLeft, atlas.getSpriteAnimation("move-front-left"));

            //TODO: Figure out how to flip X of animation
            _animation.addAnimation(Animations.WalkRight, atlas.getSpriteAnimation("move-front-left"));


            var attack_anim_left = atlas.getSpriteAnimation("attack-left");
            attack_anim_left.loop = false;
            _animation.addAnimation(Animations.AttackLeft, attack_anim_left);

            entity.addComponent(_animation);
            //setupInput();
        }

        public void onTriggerEnter(Collider other, Collider local)
        {
            throw new NotImplementedException();
        }

        public void onTriggerExit(Collider other, Collider local)
        {
            throw new NotImplementedException();
        }

        public void update()
        {
            throw new NotImplementedException();
        }
        void IUpdatable.update()
        {


            if (Input.rightMouseButtonPressed && interactable)
            {
                _start = _tilemap.worldToTilePosition(this.entity.transform.position);

                _end = _tilemap.worldToTilePosition(Input.mousePosition);

                if (_astarSearchPath != null)
                {
                    current_node = 0;
                }
                _astarSearchPath = _astarGraph.search(_start, _end);


                isDone = false;
            }

            if (_astarSearchPath != null && !isDone)
            {
                var node = _astarSearchPath[current_node];
                var x = node.X * _tilemap.tileWidth + _tilemap.tileWidth * 0.5f;
                var y = node.Y * _tilemap.tileHeight + _tilemap.tileHeight * 0.5f;
                moveDir = new Vector2((x - this.entity.transform.position.X), (y - this.entity.transform.position.Y));

                CollisionResult res;
                _mover.move(moveDir * 20 * Time.deltaTime, out res);

                if (Math.Abs(moveDir.X) <= 5 && Math.Abs(moveDir.Y) <= 5)
                {
                    if (current_node < _astarSearchPath.Count - 1)
                    {
                        current_node++;
                    }
                    else
                    {
                        moveDir = Vector2.Zero;
                        isDone = true;
                        current_node = 0;
                    }
                }
            }
            if (Math.Abs(moveDir.X) > 5)
            {
                if (moveDir.X < 0)
                    animation = Animations.WalkLeft;
                else
                    animation = Animations.WalkRight;
            }
            //else if (moveDir.X > 0)
            //animation = Animations.WalkRight;
            if (Math.Abs(moveDir.Y) > 5)
            {
                if (moveDir.Y < 0)
                    animation = Animations.WalkUp;
                else if (moveDir.Y > 0)
                    animation = Animations.WalkDown;
            }


            if (moveDir != Vector2.Zero)
            {
            }
            else
            {
                animation = Animations.Idle;
            }

            if ((!_animation.isAnimationPlaying(animation) && !_animation.isAnimationPlaying(Animations.AttackLeft)) ||
            (_animation.isAnimationPlaying(Animations.AttackLeft) && !_animation.isPlaying))
            {
                _animation.play(animation);
            }
        }

        public override void render(Graphics graphics, Camera camera)
        {
            if (_astarSearchPath != null)
            {
                foreach (var node in _astarSearchPath)
                {
                    var x = node.X * _tilemap.tileWidth + _tilemap.tileWidth * 0.5f;
                    var y = node.Y * _tilemap.tileHeight + _tilemap.tileHeight * 0.5f;

                    graphics.batcher.drawPixel(x - 1, y - 1, Color.Blue, 4);
                }
            }
            if(interactable)
            {
                _animation.color = Color.Red;
            }else
            {
                _animation.color = Color.White;
            }
        }
    }
}
