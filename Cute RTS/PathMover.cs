using Cute_RTS.Units;
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
        public bool HasArrived { get { return isDone; } }
        public Point TargetLocation { get { return target; } }

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
        private Vector2 pastLoc;
        private Vector2 pastDir;
        private Selectable selectable;
        private bool doneReroute = true;
        private int numberOfTilesWide,numberOfTilesHigh;
        private float colliderPosX;
        private float colliderPosY;
        private Point initialPositionInTileMap;
        private int pathingReroutePadding = 1;
        private Vector2 collisionPos;

        private List<Point> pathingCollisionPoints;

        public PathMover(TiledMap tilemap, string collisionlayer, Selectable selectable)
        {
            _tilemap = tilemap;
            this.selectable = selectable;
            var layer = tilemap.getLayer<TiledTileLayer>(collisionlayer);
            _astarGraph = new WeightedGridGraph(layer);
            pathingCollisionPoints = new List<Point>();
            OnArrival += PathMover_OnArrival;
        }

        private void PathMover_OnArrival()
        {
            clearObstacleNodes();
        }

        public override void onAddedToEntity()
        {
            _mover = new Mover();
            entity.addComponent(_mover);
        }

        public void clearObstacleNodes()
        {
            foreach (Point point in pathingCollisionPoints)
            {
                _astarGraph.weightedNodes.Remove(point);
            }

            pathingCollisionPoints.Clear();
        }


        void IUpdatable.update()
        {
            if (isDone) return;
            if (_astarSearchPath != null)
            {
                makeMoveStep();
            }
            else
            {
                isDone = true;
                current_node = 0;
                OnArrival?.Invoke();
            }
        }

        public void makeMoveStep()
        {
            var node = _astarSearchPath[current_node];
            var x = node.X * _tilemap.tileWidth + _tilemap.tileWidth * 0.5f;
            var y = node.Y * _tilemap.tileHeight + _tilemap.tileHeight * 0.5f;
            Vector2 moveDir = new Vector2((x - this.entity.transform.position.X), (y - this.entity.transform.position.Y));

            Vector2 dir = entity.transform.position - pastLoc;
            dir.Normalize();

            if (dir != pastDir)
            {
                pastDir = dir;
                OnDirectionChange?.Invoke(dir);
            }

            pastLoc = entity.transform.position;

            CollisionResult res;
            _mover.move(moveDir * MoveSpeed * Time.deltaTime, out res);
            if (res.collider != null)
            {
                collisionPos = entity.transform.position;
                Core.schedule(0.2f, timer =>
                { if (collisionPos == entity.transform.position)
                {
                    target = target + new Point(Nez.Random.range(-2, 3), Nez.Random.range(-2, 3));
                    Core.schedule(0.5f, t => stopMoving());
                } });
                
                
                rerouteEntity(ref res);
                OnCollision?.Invoke(ref res);
            }

            if (Math.Abs(moveDir.X) <= 5 && Math.Abs(moveDir.Y) <= 5)
            {
                BaseUnit temp = entity as BaseUnit;
                temp.setPosition(new Vector2(x, y));
                if (current_node < _astarSearchPath.Count - 1)
                {
                    current_node++;
                }
                else
                {
                    isDone = true;
                    current_node = 0;
                }
            }
        }

        public void rerouteEntity(ref CollisionResult colliderRes)
        {
            if (doneReroute)
            {

                numberOfTilesWide = (int)Math.Ceiling(colliderRes.collider.bounds.width / _tilemap.tileWidth);
                numberOfTilesHigh = (int)Math.Ceiling(colliderRes.collider.bounds.height / _tilemap.tileHeight);


                colliderPosX = colliderRes.collider.bounds.x;
                colliderPosY = colliderRes.collider.bounds.y;



                Vector2 initialPosition = new Vector2(
                    colliderPosX,
                    colliderPosY
                );
                initialPositionInTileMap = _tilemap.worldToTilePosition(initialPosition);

                Console.WriteLine(initialPositionInTileMap);


                for (int i = -pathingReroutePadding; i < numberOfTilesWide + pathingReroutePadding; i ++)
                {
                    for(int j = -pathingReroutePadding; j < numberOfTilesHigh + pathingReroutePadding; j++)
                    {
                        Point pointToAdd = new Point(
                                initialPositionInTileMap.X + i,
                                initialPositionInTileMap.Y + j);
                        if (_astarGraph.weightedNodes.Contains(pointToAdd)) continue;
                        pathingCollisionPoints.Add(pointToAdd);
                        Console.WriteLine("Added Point : " + pointToAdd);
                    }
                }

                foreach(Point point in pathingCollisionPoints)
                {
                    _astarGraph.weightedNodes.Add(point);
                }
                doneReroute = false;
                retryRoute();
            }
        }

        public bool setTargetLocation(Point target)
        {
            clearObstacleNodes();
            Point source = _tilemap.worldToTilePosition(entity.transform.position);

            target = _tilemap.worldToTilePosition(target.ToVector2());
            Console.WriteLine(target);
            _astarSearchPath = _astarGraph.search(source, target);

            if (_astarSearchPath != null)
            {
                this.target = target;
                this.source = source;
                isDone = false;
                current_node = 0;

                return true;
            }
            else
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
            collisionPos = new Vector2(-1, -1);
            isDone = true;
            target = entity.transform.position.ToPoint();
            OnArrival?.Invoke();
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

            foreach (Point point in pathingCollisionPoints)
            {
                graphics.batcher.drawHollowRect(
                    new Rectangle(
                _tilemap.tileToWorldPosition(point).ToPoint(),
                new Point(_tilemap.tileWidth, _tilemap.tileHeight)),
                Color.Blue);
            }
        }
    }
}
