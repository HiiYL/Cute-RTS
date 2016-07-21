using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Tiled;

namespace Cute_RTS
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public enum Animation
    {
        WalkUp
    }
    public class Game1 : Core
    {
        Scene.SceneResolutionPolicy policy;

        public Game1() : base(isFullScreen: false, enableEntitySystems: false)
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
            var myScene = Scene.createWithDefaultRenderer(Color.CornflowerBlue);
            var moon = myScene.content.Load<Texture2D>("moon");

            var entity = myScene.createEntity("first-sprite");
            entity.addComponent(new Sprite(moon));
            entity.transform.position = new Vector2(400, 400);


            var tiledEntity = myScene.createEntity("tiled-map-entity");
            var tiledmap = content.Load<TiledMap>("destructable-map");
            tiledEntity.addComponent(new TiledMapComponent(tiledmap,"main"));

            var atlas = myScene.content.Load<TextureAtlas>("FirstAtlas");
            var anim = atlas.getSpriteAnimation("run");

            var friction = 0.3f;
            var elasticity = 0.4f;
            var mass = 1f;
            Vector2 velocity = new Vector2(150, 0);


            var rigidbody = new ArcadeRigidbody()
                .setMass(mass)
                .setFriction(friction)
                .setElasticity(elasticity)
                .setVelocity(velocity);


            var entityTwo = myScene.createEntity("some-dude");

            entityTwo.transform.position = new Vector2(50, 50);

            var sprite = new Sprite<Animation>(Animation.WalkUp, anim);

            entityTwo.addComponent(sprite);
            entityTwo.addComponent(new SimpleMover());
            entityTwo.addComponent(new Pathfinder(tiledmap));
            entityTwo.addComponent(rigidbody);
            entityTwo.addCollider(new CircleCollider());
            sprite.play(Animation.WalkUp);





            // set the scene so Nez can take over
            Core.scene = myScene;
        }
    }
}
