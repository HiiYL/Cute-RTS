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
        public bool HasArrived { get { return _isDone; } }
        public Point TargetLocation { get { return _target; } }

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

        private bool _isDone = true;
        private int _current_node = 0;
        private Mover _mover;
        private Point _target, _source;
        private Vector2 _pastLoc;
        private Vector2 _pastDir;
        private Selectable _selectable;
        private bool _doneReroute = true;
        private int _numberOfTilesWide,_numberOfTilesHigh;
        private float _colliderPosX;
        private float _colliderPosY;
        private Point _initialPositionInTileMap;
        private int _pathingReroutePadding = 0;
        private Vector2 _collisionPos;
        private List<Point> _pathingCollisionPoints;

        public PathMover(TiledMap tilemap, string collisionlayer, Selectable selectable)
        {
            _tilemap = tilemap;
            this._selectable = selectable;
            var layer = tilemap.getLayer<TiledTileLayer>(collisionlayer);
            _astarGraph = new WeightedGridGraph(layer);
            _pathingCollisionPoints = new List<Point>();
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
            foreach (Point point in _pathingCollisionPoints)
            {
                _astarGraph.weightedNodes.Remove(point);
            }

            _pathingCollisionPoints.Clear();
        }


        void IUpdatable.update()
        {
            if (_isDone || entity == null) return;
            if (_astarSearchPath != null)
            {
                makeMoveStep();
            }
            else
            {
                _isDone = true;
                _current_node = 0;
                OnArrival?.Invoke();
            }
        }

        public void makeMoveStep()
        {
            var node = _astarSearchPath[_current_node];
            var x = node.X * _tilemap.tileWidth + _tilemap.tileWidth * 0.5f;
            var y = node.Y * _tilemap.tileHeight + _tilemap.tileHeight * 0.5f;
            Vector2 moveDir = new Vector2((x - this.entity.transform.position.X), (y - this.entity.transform.position.Y));

            Vector2 dir = entity.transform.position - _pastLoc;
            dir.Normalize();

            if (dir != _pastDir)
            {
                _pastDir = dir;
                OnDirectionChange?.Invoke(dir);
            }

            _pastLoc = entity.transform.position;

            CollisionResult res;
            _mover.move(moveDir * MoveSpeed * Time.deltaTime, out res);
            if (res.collider != null)
            {
                _collisionPos = entity.transform.position;
                Core.schedule(0.2f, timer =>
                {
                    if (_collisionPos == entity.transform.position)
                    {
                        _target = _target + new Point(Nez.Random.range(-2, 3), Nez.Random.range(-2, 3));
                        Core.schedule(0.5f, t => stopMoving());
                    }
                });
                
                
                rerouteEntity(ref res);
                OnCollision?.Invoke(ref res);
            }

            if (_astarSearchPath == null)
            {
                Console.WriteLine("How very strange. In rare cases the search path becomes null midway through...");
                return;
            }

            if (Math.Abs(moveDir.X) <= 5 && Math.Abs(moveDir.Y) <= 5)
            {
                BaseUnit temp = entity as BaseUnit;
                temp.setPosition(new Vector2(x, y));
                if (_current_node < _astarSearchPath.Count - 1)
                {
                    _current_node++;
                }
                else
                {
                    _isDone = true;
                    _current_node = 0;
                }
            }
        }

        public void rerouteEntity(ref CollisionResult colliderRes)
        {
            if (_doneReroute)
            {

                _numberOfTilesWide = (int)Math.Ceiling(colliderRes.collider.bounds.width / _tilemap.tileWidth);
                _numberOfTilesHigh = (int)Math.Ceiling(colliderRes.collider.bounds.height / _tilemap.tileHeight);


                _colliderPosX = colliderRes.collider.bounds.x;
                _colliderPosY = colliderRes.collider.bounds.y;



                Vector2 initialPosition = new Vector2(
                    _colliderPosX,
                    _colliderPosY
                );
                _initialPositionInTileMap = _tilemap.worldToTilePosition(initialPosition);
                
                for (int i = -_pathingReroutePadding; i < _numberOfTilesWide + _pathingReroutePadding; i ++)
                {
                    for(int j = -_pathingReroutePadding; j < _numberOfTilesHigh + _pathingReroutePadding; j++)
                    {
                        Point pointToAdd = new Point(
                                _initialPositionInTileMap.X + i,
                                _initialPositionInTileMap.Y + j);
                        if (_astarGraph.weightedNodes.Contains(pointToAdd))
                            continue;
                        _pathingCollisionPoints.Add(pointToAdd);
                    }
                }

                foreach(Point point in _pathingCollisionPoints)
                {
                    _astarGraph.weightedNodes.Add(point);
                }
                _doneReroute = false;
                retryRoute();
            }
        }

        public bool setTargetLocation(Point target)
        {
            clearObstacleNodes();
            Point source = _tilemap.worldToTilePosition(entity.transform.position);

            target = _tilemap.worldToTilePosition(target.ToVector2());
            _astarSearchPath = _astarGraph.search(source, target);

            if (_astarSearchPath != null)
            {
                this._target = target;
                this._source = source;
                _isDone = false;
                _current_node = 0;

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
            _astarSearchPath = _astarGraph.search(source, this._target);
            if (_astarSearchPath != null)
            {
                this._source = source;
                _isDone = false;
                _current_node = 0;
                _doneReroute = true;
                return true;
            }
            else
            {
                return false;
            }

        }

        public void stopMoving()
        {
            if (entity != null)
            {
                _collisionPos = new Vector2(-1, -1);
                _isDone = true;
                _target = entity.transform.position.ToPoint();
                OnArrival?.Invoke();
            }
        }


        public override void render(Graphics graphics, Camera camera)
        {
            if (_isDone) return;

            if (IsRenderPath && _astarSearchPath != null)
            {
                foreach (var node in _astarSearchPath)
                {
                    var x = node.X * _tilemap.tileWidth + _tilemap.tileWidth * 0.5f;
                    var y = node.Y * _tilemap.tileHeight + _tilemap.tileHeight * 0.5f;

                    graphics.batcher.drawPixel(x - 1, y - 1, Color.LimeGreen, 3);
                }
            }

            foreach (Point point in _pathingCollisionPoints)
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
