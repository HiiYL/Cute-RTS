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
        private bool doneReroute = true;
        private int numberOfTilesWide,numberOfTilesHigh;
        private float colliderPosX;
        private float colliderPosY;
        private Point initialPositionInTileMap;
        private int pathingReroutePadding = 2;

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

            OnDirectionChange?.Invoke(moveDir);


            CollisionResult res;
            _mover.move(moveDir * MoveSpeed * Time.deltaTime, out res);
            if (res.collider != null)
            {
                rerouteEntity(res);
                OnCollision?.Invoke(ref res);
            }

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

        public void rerouteEntity(CollisionResult colliderRes)
        {
            if (doneReroute)
            {

                numberOfTilesWide = (int)Math.Ceiling(colliderRes.collider.bounds.width / _tilemap.tileWidth);
                numberOfTilesHigh = (int)Math.Ceiling(colliderRes.collider.bounds.height / _tilemap.tileHeight);
                Console.WriteLine(numberOfTilesWide);
                Console.WriteLine(numberOfTilesHigh);

                colliderPosX = colliderRes.collider.bounds.x;
                colliderPosY = colliderRes.collider.bounds.y;

                Vector2 initialPosition = new Vector2(
                    colliderPosX,
                    colliderPosY
                );
                initialPositionInTileMap = _tilemap.worldToTilePosition(initialPosition);




                for (int i = -pathingReroutePadding; i < numberOfTilesWide + pathingReroutePadding; i ++)
                {
                    for(int j = -pathingReroutePadding; j < numberOfTilesHigh + pathingReroutePadding; j++)
                    {

                        _astarGraph.weightedNodes.Add(
                            new Point(
                                initialPositionInTileMap.X + i,
                                initialPositionInTileMap.Y + j)
                                );
                    }
                }
                doneReroute = false;
                retryRoute();
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

        public bool retryRoute()
        {
            Point source = _tilemap.worldToTilePosition(entity.transform.position);
            _astarSearchPath = _astarGraph.search(source, this.target);
            if (_astarSearchPath != null)
            {
                this.source = source;
                isDone = false;
                current_node = 0;
                doneReroute = true;
                return true;
            }
            else
            {
                return false;
            }

        }

        public void stopMoving()
        {
            isDone = true;
            target = entity.transform.position.ToPoint();
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
            for (int i = -pathingReroutePadding; i < numberOfTilesWide + pathingReroutePadding; i++)
            {
                for (int j = -pathingReroutePadding; j < numberOfTilesHigh + pathingReroutePadding; j++)
                {
                    graphics.batcher.drawRect(
                        new Rectangle(
                            _tilemap.tileToWorldPositionX(initialPositionInTileMap.X + i),
                            _tilemap.tileToWorldPositionX(initialPositionInTileMap.Y + j),
                _tilemap.tileWidth,
                _tilemap.tileHeight),
                        Color.Blue);

                }
            }
            graphics.batcher.drawRect(new Rectangle(25,25,150,150), Color.Red);
        }
    }
}
