using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS
{
    class PathMover : RenderableComponent, IUpdatable
    {
        // make sure we arent culled
        public override float width { get { return 1000; } }
        public override float height { get { return 1000; } }
        public bool IsRenderPath { get; set; } = true;
        public int MoveSpeed { get; set; } = 15;

        // Events:
        public delegate void OnCollisionHandler(ref CollisionResult res);
        public event OnCollisionHandler OnCollision;
        public delegate void OnArrivalHandler();
        public event OnArrivalHandler OnArrival;
        public delegate void OnDirectionChangeHandler(Vector2 moveDir);
        public event OnDirectionChangeHandler OnDirectionChange;

        WeightedGridGraph _astarGraph;
        List<Point> _astarSearchPath;

        TiledMap _tilemap;

        bool isDone = true;
        int current_node = 0;
        private Mover _mover;
        private Point target, source;
        private Vector2 pastMoveDir;
        private Selectable selectable;

        public PathMover(TiledMap tilemap, string collisionlayer, Selectable selectable)
        {
            _tilemap = tilemap;
            this.selectable = selectable;
            var layer = tilemap.getLayer<TiledTileLayer>(collisionlayer);
            _astarGraph = new WeightedGridGraph(layer);
        }

        public override void onAddedToEntity()
        {
            _mover = new Mover();
            entity.addComponent(_mover);
        }


        void IUpdatable.update()
        {
            if (Input.rightMouseButtonPressed && selectable.IsSelected)
            {
                gotoLocation(Input.mousePosition.ToPoint());
                return;
            }

            if (isDone) return;

            var node = _astarSearchPath[current_node];
            var x = node.X * _tilemap.tileWidth + _tilemap.tileWidth * 0.5f;
            var y = node.Y * _tilemap.tileHeight + _tilemap.tileHeight * 0.5f;
            Vector2 moveDir = new Vector2((x - this.entity.transform.position.X), (y - this.entity.transform.position.Y));


            if (((int) moveDir.X ^ (int) pastMoveDir.X) < 0 
                || ((int)moveDir.Y ^ (int)pastMoveDir.Y) < 0)
            {
                pastMoveDir = moveDir;
                OnDirectionChange?.Invoke(moveDir);
            }


            CollisionResult res;
            _mover.move(moveDir * MoveSpeed * Time.deltaTime, out res);

            if (Math.Abs(moveDir.X) <= 5 && Math.Abs(moveDir.Y) <= 5)
            {
                if (current_node < _astarSearchPath.Count - 1)
                {
                    current_node++;
                }
                else
                {
                    isDone = true;
                    current_node = 0;
                    OnArrival?.Invoke();
                }
            }
        }

        public bool gotoLocation(Point target)
        {
            Point source = _tilemap.worldToTilePosition(entity.transform.position);
            target = _tilemap.worldToTilePosition(Input.mousePosition);
            _astarSearchPath = _astarGraph.search(source, target);

            if (_astarSearchPath != null)
            {
                this.target = target;
                this.source = source;
                isDone = false;
                current_node = 0;

                return true;
            } else
            {
                return false;
            }

        }


        public override void render(Graphics graphics, Camera camera)
        {
            if (isDone) return;

            if (IsRenderPath && _astarSearchPath != null)
            {
                foreach (var node in _astarSearchPath)
                {
                    var x = node.X * _tilemap.tileWidth + _tilemap.tileWidth * 0.5f;
                    var y = node.Y * _tilemap.tileHeight + _tilemap.tileHeight * 0.5f;

                    graphics.batcher.drawPixel(x - 1, y - 1, Color.Blue, 4);
                }
            }
        }
    }
}
