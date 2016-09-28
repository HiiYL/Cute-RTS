using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Scenes
{
    class BaseScene : Scene
    {
        public const int SCREEN_SPACE_RENDER_LAYER = 999;
        public UICanvas canvas;

        public Table _table;
        ScreenSpaceRenderer _screenSpaceRenderer;

        public BaseScene(bool addExcludeRenderer = true, bool needsFullRenderSizeForUI = false) : base()
		{
            // setup one renderer in screen space for the UI and then (optionally) another renderer to render everything else
            if (needsFullRenderSizeForUI)
            {
                // dont actually add the renderer since we will manually call it later
                _screenSpaceRenderer = new ScreenSpaceRenderer(100, SCREEN_SPACE_RENDER_LAYER);
                _screenSpaceRenderer.shouldDebugRender = false;
            }
            else
            {
                addRenderer(new ScreenSpaceRenderer(100, SCREEN_SPACE_RENDER_LAYER));
            }

            if (addExcludeRenderer)
                addRenderer(new RenderLayerExcludeRenderer(0, SCREEN_SPACE_RENDER_LAYER));

        }
        public override void initialize()
        {
            //Console.WriteLine("Called!");
            // create our canvas and put it on the screen space render layer
            canvas = createEntity("ui").addComponent(new UICanvas());
            canvas.isFullScreen = true;
            canvas.renderLayer = SCREEN_SPACE_RENDER_LAYER;
            setupSceneSelector();

        }
        void setupSceneSelector()
        {
            _table = canvas.stage.addElement(new Table());
            _table.setFillParent(true).right().top();

            var topButtonStyle = new TextButtonStyle(new PrimitiveDrawable(Color.Black, 10f), new PrimitiveDrawable(Color.Yellow), new PrimitiveDrawable(Color.DarkSlateBlue))
            {
                downFontColor = Color.Black
            };

            _table.row().setPadTop(10);
            var checkbox = _table.add(new CheckBox("Debug Render", new CheckBoxStyle
            {
                checkboxOn = new PrimitiveDrawable(30, Color.Green),
                checkboxOff = new PrimitiveDrawable(30, Color.MonoGameOrange)
            })).getElement<CheckBox>();
            checkbox.onChanged += enabled => Core.debugRenderEnabled = enabled;
            checkbox.isChecked = Core.debugRenderEnabled;
            _table.row().setPadTop(30);

            var buttonStyle = new TextButtonStyle(new PrimitiveDrawable(new Color(78, 91, 98), 10f), new PrimitiveDrawable(new Color(244, 23, 135)), new PrimitiveDrawable(new Color(168, 207, 115)))
            {
                downFontColor = Color.Black
            };
        }

        void addInstructionText(string text)
        {
            var instructionsEntity = createEntity("instructions");
            instructionsEntity.addComponent(new Text(Graphics.instance.bitmapFont, text, new Vector2(10, 10), Color.White))
                .setRenderLayer(SCREEN_SPACE_RENDER_LAYER);
        }

    }
}
