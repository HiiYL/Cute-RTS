using Cute_RTS.Scenes;
using Cute_RTS.Units;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nez.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Cute_RTS
{
    class SelectionProcessingSystem : ProcessingSystem
    {
        private Vector2 initialPos = Vector2.Zero;
        private bool onlyOnce = true;
        private Table _table;
        private Image _image;

        public override void process()
        {
            if (Input.leftMouseButtonReleased)
            {
                if (initialPos == Vector2.Zero)
                {
                    Collider v = Physics.overlapRectangle(new RectangleF(Input.mousePosition.X, Input.mousePosition.Y, 5, 5));
                    if (v != null)
                    {
                        var humanFootman = v.entity.getComponent<HumanFootman>();
                        if (humanFootman != null)
                        {
                            humanFootman.interactable = !humanFootman.interactable;
                        }
                    }
                }else
                {
                    _image.setVisible(false);

                    //PROBLEM, SETBOUNDS AND DRAW RECTANGLE DOESNT WORK WITH NEGATIVE X VALUE AND POSITIVE Y VALUE OR VICE VERSA
                    //RATHER UGLY WORKAROUND, BUT IT WORKS
                    float X_begin, X_length;
                    float Y_begin, Y_length;
                    if (Input.mousePosition.X > initialPos.X)
                    {
                        X_begin = initialPos.X;
                        X_length = (Input.mousePosition.X - initialPos.X);
                    }
                    else
                    {
                        X_begin = Input.mousePosition.X;
                        X_length = (initialPos.X - Input.mousePosition.X);
                    }
                    if (Input.mousePosition.Y > initialPos.Y)
                    {
                        Y_begin = initialPos.Y;
                        Y_length = (Input.mousePosition.Y - initialPos.Y);
                    }
                    else
                    {
                        Y_begin = Input.mousePosition.Y;
                        Y_length = (initialPos.Y - Input.mousePosition.Y);
                    }
                    var colliders = new HashSet<Collider>(Physics.boxcastBroadphase(new RectangleF(X_begin, Y_begin, X_length, Y_length)));
                    if (colliders != null)
                    {
                        foreach(var v in colliders) {
                            var humanFootman = v.entity.getComponent<HumanFootman>();
                            if (humanFootman != null)
                            {
                                humanFootman.interactable = !humanFootman.interactable;
                            }
                        }

                    }
                    initialPos = Vector2.Zero;
                }
            }
            //if (((BaseScene)scene).canvas != null)
            //{
            if (onlyOnce)
            {
                UICanvas canvas = ((BaseScene)scene).canvas;
                _table = canvas.stage.addElement(new Table());
                _table.setFillParent(true);
                _image = _table.addElement(new Image(scene.content.Load<Texture2D>("selection_box")));
                _image.setVisible(false);

                onlyOnce = false;
            }
            if (Input.leftMouseButtonPressed)
            {
                initialPos = Input.mousePosition;
                _image.setVisible(true);
            }
            if (Input.leftMouseButtonDown)
            {

                //PROBLEM, SETBOUNDS AND DRAW RECTANGLE DOESNT WORK WITH NEGATIVE X VALUE AND POSITIVE Y VALUE OR VICE VERSA
                //RATHER UGLY WORKAROUND, BUT IT WORKS
                float X_begin, X_length;
                float Y_begin, Y_length;
                if(Input.mousePosition.X > initialPos.X)
                {
                    X_begin = initialPos.X;
                    X_length = (Input.mousePosition.X - initialPos.X);
                }else
                {
                    X_begin = Input.mousePosition.X;
                    X_length = (initialPos.X - Input.mousePosition.X);
                }
                if (Input.mousePosition.Y > initialPos.Y)
                {
                    Y_begin = initialPos.Y;
                    Y_length = (Input.mousePosition.Y - initialPos.Y);
                }
                else
                {
                    Y_begin = Input.mousePosition.Y;
                    Y_length = (initialPos.Y - Input.mousePosition.Y);
                }


                _image.setBounds(X_begin, Y_begin, X_length, Y_length);

            }
           // }
        }
    }
}
