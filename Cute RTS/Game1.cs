using Cute_RTS.Scenes;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Tiled;

namespace Cute_RTS
{
    public enum RTSCollisionLayer
    {
        None,
        Units,
        Map
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Core
    {
        Scene.SceneResolutionPolicy policy;

        public Game1() : base(isFullScreen: false, enableEntitySystems: true)
        {
            policy = Scene.SceneResolutionPolicy.ShowAllPixelPerfect;
            Scene.setDefaultDesignResolution(1280, 720, policy);
            Window.AllowUserResizing = true;
        }



        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            Window.AllowUserResizing = true;

            // create our Scene with the DefaultRenderer and a clear color of CornflowerBlue
            var myScene = new BaseScene(true);

            var tiledEntity = myScene.createEntity("tiled-map-entity");
            var tiledmap = content.Load<TiledMap>("cute-map");
            var tmc = new TiledMapComponent(tiledmap, "Stuff");

            var tiledMapComponent = tiledEntity.addComponent(tmc);
            Flags.setFlagExclusive(ref tiledMapComponent.physicsLayer, (int) RTSCollisionLayer.Map);
            tiledMapComponent.renderLayer = 10;

            //TextureAtlas baldyTexture = content.Load<TextureAtlas>("BaldyAtlas");
            TextureAtlas baldyTexture = content.Load<TextureAtlas>("CatAtlas");
            Texture2D catSelection = content.Load<Texture2D>("Units/Cat/cat-selection");


            Cat fatman = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            myScene.addEntity(fatman);
            fatman.transform.position = new Vector2(0, 500);
            fatman.gotoLocation(new Point(300, 500));

            Cat fatman2 = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            myScene.addEntity(fatman2);
            fatman2.transform.position = new Vector2(0, 100);
            fatman2.gotoLocation(new Point(300, 100));


            Cat fatman3 = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            myScene.addEntity(fatman3);
            fatman3.transform.position = new Vector2(0, 400);
            fatman3.gotoLocation(new Point(300, 400));

            var selectionComponent = tiledEntity.addComponent(Selector.getSelector());
            selectionComponent.renderLayer = -5;

            // set the scene so Nez can take over
            Core.scene = myScene;
        }
    }
}
