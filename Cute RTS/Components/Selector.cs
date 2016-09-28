using Cute_RTS.Scenes;
using Cute_RTS.Structures;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Cute_RTS
{
    class Selector : RenderableComponent, IUpdatable
    {
        // make sure we arent culled
        public override float width { get { return 1000; } }
        public override float height { get { return 1000; } }
        public Texture2D TargetTex { get; set; }
        public Player ActivePlayer { get; set; }

        private bool isAttackBtnClicked = false;
        
        public delegate void SelectionHandler(IReadOnlyList<Selectable> sels);
        public event SelectionHandler OnSelectionChanged;

        private Rectangle _selectionBoundary;
        private bool _isSelectionBox = false;
        private Vector2 _initialPos = Vector2.Zero;
        private Color _selectionColor;
        private static Selector selector;
        private List<Selectable> _selectables;
        private List<Selectable> _cachedSelectables;
        private bool _displayTarget = false;
        private Vector2 _displayTargetPoint;
        private Timer _targetTexTimer;
        private ImageButton _catBtn;

        private Rectangle selectableRect = new Rectangle(new Point(0, 0), new Point(800, 480));

        public IReadOnlyList<Selectable> Selectables {
            get { return _selectables.AsReadOnly(); }
        }

        private Selector()
        {
            _targetTexTimer = new Timer(1000);
            _targetTexTimer.Elapsed += _targetTexTimer_Elapsed;
            _selectables = new List<Selectable>();
            _selectionColor = Color.DarkBlue;
            _selectionColor.A = (byte)0.1;
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
            _cachedSelectables = new List<Selectable>(_selectables);
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
            if (_isSelectionBox)
            {
                graphics.batcher.drawRect(_selectionBoundary, _selectionColor);
            }

            if (_displayTarget)
            {
                graphics.batcher.draw(TargetTex, _displayTargetPoint);
            }
        }

        public void onStopMovingBtnPressed(Button button)
        {
            foreach (var s in _cachedSelectables)
            {
                var b = s.entity as BaseUnit;
                if (b != null)
                    b.stopMoving();
            }
        }

        public void onAttackBtnPressed(Button button)
        {
            //Console.WriteLine("Attack Btn Changed!");
            isAttackBtnClicked = !isAttackBtnClicked;
            
        }


        void IUpdatable.update()
        {
            if(_selectables.Count > 0)
            {
                ((GameScene)entity.scene)._selectedUnitTable.setVisible(true);
            }else
            {
                ((GameScene)entity.scene)._selectedUnitTable.setVisible(false);
            }
            if (Input.rightMouseButtonPressed && _selectables.Count > 0)
            {
                Collider v = Physics.overlapCircle(Input.mousePosition, 5f, layerMask: (int)RTSCollisionLayer.Map);
                if (v != null)
                {
                    if (v.entity is Attackable)
                    {
                        var g = v.entity as Attackable;
                        
                        g.Select.playClickSelectAnimation();
                        foreach (var s in _selectables)
                        {
                            var b = s.entity as BaseUnit;
                            if (b == null) continue;

                            if (b.UnitPlayer.isMyUnit(g))
                            {
                                b.followUnit(g);
                            }
                            else
                            {
                                b.attackUnit(g);
                            }
                        }
                    } else if (v.entity is CaptureFlag)
                    {
                        var fl = v.entity as CaptureFlag;

                        fl.Select.playClickSelectAnimation();
                        foreach (var s in _selectables)
                        {
                            var b = s.entity as BaseUnit;
                            if (b == null) continue;

                            b.captureFlag(fl);
                        }
                    }
                    
                }
                else
                {
                    foreach (var s in _selectables)
                    {
                        var b = s.entity as BaseUnit;
                        if (b == null) continue;
                        if (isAttackBtnClicked)
                        {
                            b.attackLocation(Input.mousePosition.ToPoint()); // test attack location
                            isAttackBtnClicked = false;
                        }
                        else
                        {
                            b.gotoLocation(Input.mousePosition.ToPoint());
                            if (TargetTex != null)
                            {
                                _displayTarget = true;
                                _displayTargetPoint = Input.mousePosition - new Vector2(TargetTex.Width / 2, TargetTex.Height / 2);

                                // restart timer
                                _targetTexTimer.Stop();
                                _targetTexTimer.Start();

                            }
                        }
                    }
                }
                return;
            }


            if (Input.isKeyReleased(Keys.S))
            {
                foreach (var s in _selectables)
                {
                    var b = s.entity as BaseUnit;
                    if(b != null)
                      b.stopMoving();

                }
                return;
            }

            if (Input.leftMouseButtonPressed)
            {
                _initialPos = Input.mousePosition;
            } else if (Input.leftMouseButtonDown)
            {
                _isSelectionBox = true;
                _selectionBoundary = new Rectangle(
                    (int) Math.Min(_initialPos.X, Input.mousePosition.X),
                    (int) Math.Min(_initialPos.Y, Input.mousePosition.Y),
                    (int) Math.Abs(Input.mousePosition.X - _initialPos.X),
                    (int) Math.Abs(Input.mousePosition.Y - _initialPos.Y));
            } else if (Input.leftMouseButtonReleased)
            {
                if (selectableRect.Contains(Input.mousePosition))
                    getSelector().deselectAll();
                _isSelectionBox = false;
                if (_initialPos == Input.mousePosition)
                {
                    // layer mask makes sure the map colliders are not selected:
                    Collider v = Physics.overlapCircle(Input.mousePosition, 5f, layerMask: (int)RTSCollisionLayer.Map);
                    if (v != null)
                    {
                        // prevent you from selecting (and thus controlling) enemy units
                        var ent = v.entity as Attackable;
                        if (ent != null && ent.UnitPlayer != ActivePlayer) return;
                        if (v.entity is CaptureFlag) return;

                        //Console.WriteLine("IS IT MAIN BASE?");
                        setupUI(v.entity);
                        var s = v.entity.getComponent<Selectable>();
                        if (s != null)
                        {
                            s.IsSelected = true;
                        }
                    }
                } else
                {
                    RectangleF selectionBoundaryF = new RectangleF(
                        _selectionBoundary.X,
                        _selectionBoundary.Y,
                        _selectionBoundary.Width,
                        _selectionBoundary.Height);
                    // layer mask makes sure the map colliders are not selected:
                    var colliders = new HashSet<Collider>(Physics.boxcastBroadphase(selectionBoundaryF, layerMask:(int) RTSCollisionLayer.Map));
                    if (colliders != null)
                    {
                        foreach (var v in colliders)
                        {
                            // prevent you from selecting (and thus controlling) enemy units
                            var ent = v.entity as Attackable;
                            if (ent != null && ent.UnitPlayer != ActivePlayer) continue;
                            if (v.entity is CaptureFlag) continue;

                            setupUI(v.entity);
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
        private void setupUI(Entity entity)
        {
            if (_catBtn != null)
            {
                _catBtn.setIsVisible((entity is MainBase));
            }else
            if (entity is MainBase)
            {
                //Console.WriteLine("YES!!");
                var catTex = entity.scene.content.Load<Texture2D>("train-cat");

                _catBtn = new ImageButton(new ImageButtonStyle(new PrimitiveDrawable(Color.Red), new PrimitiveDrawable(Color.Black), new PrimitiveDrawable(Color.Blue),
                    new SubtextureDrawable(catTex), new SubtextureDrawable(catTex), new SubtextureDrawable(catTex)));
                ((GameScene)entity.scene)._selectedUnitTable.add(_catBtn).setMinWidth(100).setMinHeight(30);
                _catBtn.onClicked += ((MainBase)entity).onTrainUnitBtnPressed;
            }
        }

        private void _targetTexTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_displayTarget) _displayTarget = false;
            _targetTexTimer.Stop();
        }
    }
}
