using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS
{
    class Selector : RenderableComponent, IUpdatable
    {
        // make sure we arent culled
        public override float width { get { return 1000; } }
        public override float height { get { return 1000; } }
        
        public delegate void SelectionHandler(IReadOnlyList<Selectable> sels);
        public event SelectionHandler OnSelectionChanged;

        private Rectangle selectionBoundary;
        private bool isSelectionBox = false;
        private Vector2 initialPos = Vector2.Zero;
        private Color selectionColor;
        private static Selector selector;
        private List<Selectable> _selectables;

        public IReadOnlyList<Selectable> Selectables {
            get { return _selectables.AsReadOnly(); }
        }

        private Selector()
        {
            _selectables = new List<Selectable>();
            selectionColor = Color.DarkBlue;
            selectionColor.A = (byte)0.1;
        }

        public void add(Selectable sel)
        {
            _selectables.Add(sel);
            OnSelectionChanged?.Invoke(Selectables);
        }

        public bool remove(Selectable sel)
        {
            bool result = _selectables.Remove(sel);

            if (result) OnSelectionChanged?.Invoke(Selectables);

            return result;
        }

        public void deselectAll()
        {
            if (_selectables.Count == 0) return;

            foreach (var s in _selectables.ToList())
            {
                s.IsSelected = false;
            }

            _selectables.Clear();
            OnSelectionChanged?.Invoke(Selectables);
        }

        // singleton
        public static Selector getSelector()
        {
            if (selector == null)
            {
                selector = new Selector();
            }

            return selector;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            if (isSelectionBox)
            {
                graphics.batcher.drawRect(selectionBoundary, selectionColor);
            }
        }


        void IUpdatable.update()
        {
            if (Input.rightMouseButtonPressed && _selectables.Count > 0)
            {
                foreach (var s in _selectables)
                {
                    var b = s.entity as BaseUnit;
                    Collider v = Physics.overlapCircle(Input.mousePosition, 5f, layerMask: (int)RTSCollisionLayer.Map);
                    if (v != null)
                    {
                        var g = v.entity as BaseUnit;
                        b.followUnit(g);
                    } else
                    {
                        b.gotoLocation(Input.mousePosition.ToPoint());
                    }
                }
                return;
            }

            if (Input.isKeyReleased(Keys.S))
            {
                foreach (var s in _selectables)
                {
                    var b = s.entity as BaseUnit;
                    b.stopMoving();

                }
                return;
            }

            if (Input.leftMouseButtonPressed)
            {
                initialPos = Input.mousePosition;
            } else if (Input.leftMouseButtonDown)
            {
                isSelectionBox = true;
                selectionBoundary = new Rectangle(
                    (int) Math.Min(initialPos.X, Input.mousePosition.X),
                    (int) Math.Min(initialPos.Y, Input.mousePosition.Y),
                    (int) Math.Abs(Input.mousePosition.X - initialPos.X),
                    (int) Math.Abs(Input.mousePosition.Y - initialPos.Y));
            } else if (Input.leftMouseButtonReleased)
            {
                getSelector().deselectAll();
                isSelectionBox = false;
                if (initialPos == Input.mousePosition)
                {
                    // layer mask makes sure the map colliders are not selected:
                    Collider v = Physics.overlapCircle(Input.mousePosition, 5f, layerMask: (int)RTSCollisionLayer.Map);
                    if (v != null)
                    {
                        var s = v.entity.getComponent<Selectable>();
                        if (s != null)
                        {
                            s.IsSelected = true;
                        }
                    }
                } else
                {

                    RectangleF selectionBoundaryF = new RectangleF(
                        selectionBoundary.X,
                        selectionBoundary.Y,
                        selectionBoundary.Width,
                        selectionBoundary.Height);
                    // layer mask makes sure the map colliders are not selected:
                    var colliders = new HashSet<Collider>(Physics.boxcastBroadphase(selectionBoundaryF, layerMask:(int) RTSCollisionLayer.Map));
                    if (colliders != null)
                    {
                        foreach (var v in colliders)
                        {
                            var selectable = v.entity.getComponent<Selectable>();
                            if (selectable != null)
                            {
                                selectable.IsSelected = true;
                            }
                        }

                    }
                }
            }

            
        }


    }
}
