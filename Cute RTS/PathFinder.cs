using Microsoft.Xna.Framework;
using Nez;
using Nez.AI.Pathfinding;
using Nez.Tiled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS
{
    /// <summary>
    /// simple Component that finds a path on click and displays it via a series of rectangles
    /// </summary>
    public class Pathfinder : RenderableComponent, IUpdatable
    {
        // make sure we arent culled
        public override float width { get { return 1000; } }
        public override float height { get { return 1000; } }

        UnweightedGridGraph _gridGraph;
        List<Point> _breadthSearchPath;

        WeightedGridGraph _astarGraph;
        List<Point> _astarSearchPath;

        TiledMap _tilemap;
        Point _start, _end;

        bool isDone = false;

        int current_node = 0;
        private Mover _mover;

        public Pathfinder(TiledMap tilemap)
        {
            _tilemap = tilemap;
            var layer = tilemap.getLayer<TiledTileLayer>("Stuff");
            _start = new Point(1, 1);
            _end = new Point(10, 10);

            _astarGraph = new WeightedGridGraph(layer);
            _astarSearchPath = _astarGraph.search(_start, _end);

            Debug.drawTextFromBottom = true;
        }

        public override void onAddedToEntity()
        {
            _mover = new Mover();
            entity.addComponent(_mover);
        }


            void IUpdatable.update()
        {
            // on left click set our path end time
                // on right click set our path start time
            if (Input.rightMouseButtonPressed)
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
                Vector2 moveDir = new Vector2((x - this.entity.transform.position.X), (y - this.entity.transform.position.Y));

                CollisionResult res;
                _mover.move(moveDir * 20 * Time.deltaTime, out res);

                if (Math.Abs(moveDir.X) <= 5 && Math.Abs(moveDir.Y) <= 5)
                {
                    if (current_node < _astarSearchPath.Count - 1)
                    {
                        current_node++;
                    }else
                    {
                        isDone = true;
                        current_node = 0;
                    }
                }
                //Core.startCoroutine(moveTowardsNextNode(_astarSearchPath));
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
        }
        

    }
}
