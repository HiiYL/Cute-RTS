using Cute_RTS.Scenes;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Tiled;
using System.Collections.Generic;
using System;

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
    public class MainGame : Core
    {

        public const int SCREEN_DEFAULT_WIDTH = 800;
        public const int SCREEN_DEFAULT_HEIGHT = 580;

        public const int SCREEN_WIDTH = SCREEN_DEFAULT_WIDTH;
        public const int SCREEN_HEIGHT = (SCREEN_DEFAULT_HEIGHT);
        private Scene.SceneResolutionPolicy policy;

        public MainGame() : base(isFullScreen: false, enableEntitySystems: true)
        {
            policy = Scene.SceneResolutionPolicy.ShowAllPixelPerfect;
            Scene.setDefaultDesignResolution(SCREEN_DEFAULT_WIDTH, SCREEN_DEFAULT_HEIGHT, policy);
            Screen.setSize(SCREEN_WIDTH, SCREEN_HEIGHT);
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

            // create our Scene with the DefaultRenderer and a clear color of CornflowerBlue
            var myScene = new GameScene();

            // set the scene so Nez can take over
            Core.scene = myScene;
        }


    }
}
