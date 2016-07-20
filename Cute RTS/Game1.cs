using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;

namespace Cute_RTS
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Core
    {

        public Game1() : base(width: 1280, height: 720, isFullScreen: false, enableEntitySystems: false)
        { }
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
            var myScene = Scene.createWithDefaultRenderer(Color.CornflowerBlue);
            var moon = myScene.content.Load<Texture2D>("moon");

            var entity = myScene.createEntity("first-sprite");
            entity.addComponent(new Sprite(moon));
            entity.transform.position = new Vector2(400, 400);

            var atlas = myScene.content.Load<TextureAtlas>("FirstAtlas");
            var anim = atlas.getSpriteAnimation("run");
            var entityTwo = myScene.createEntity("some-dude");
            var sprite = new Sprite<int>(3, anim);
            entity.addComponent(sprite);
            sprite.play(3);


            // set the scene so Nez can take over
            Core.scene = myScene;
        }
    }
}
