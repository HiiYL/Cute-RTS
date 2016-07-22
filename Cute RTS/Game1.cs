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
            var moon = myScene.content.Load<Texture2D>("moon");

            myScene.addEntityProcessor(new SelectionProcessingSystem());

            var entity = myScene.createEntity("first-sprite");
            entity.addComponent(new Sprite(moon));
            entity.transform.position = new Vector2(400, 400);


            var tiledEntity = myScene.createEntity("tiled-map-entity");
            var tiledmap = content.Load<TiledMap>("cute-map");
            var tiledMapComponent = tiledEntity.addComponent(new TiledMapComponent(tiledmap,"Stuff"));

            tiledMapComponent.renderLayer = 10;

            //tiledMapDetailsComp.material = Material.stencilWrite();
            //tiledMapDetailsComp.material.effect = content.loadNezEffect<SpriteAlphaTestEffect>();

            var atlas = myScene.content.Load<TextureAtlas>("FirstAtlas");
            var anim = atlas.getSpriteAnimation("run");

            var friction = 0.3f;
            var elasticity = 0.4f;
            var mass = 1f;

            Vector2 velocity = new Vector2(0, 0);

            var playerEntity = myScene.createEntity("player");
            playerEntity.addComponent(new HumanFootman(tiledmap, false));
            var collider = playerEntity.colliders.add(new BoxCollider(-42,-42,84,84));

            var npc = myScene.createEntity("npc");
            npc.addComponent(new HumanFootman(tiledmap,false));
            var colliderNPC = npc.colliders.add(new BoxCollider(-42, -42, 84, 84));

            /*

            */


            /*
            var entityTwo = myScene.createEntity("some-dude");

            entityTwo.transform.position = new Vector2(50, 50);

            var sprite = new Sprite<Animation>(Animation.WalkUp, anim);

            entityTwo.addComponent(sprite);
            entityTwo.addComponent(new SimpleMover());
            entityTwo.addComponent(new Pathfinder(tiledmap));
            entityTwo.addComponent(rigidbody);
            entityTwo.addCollider(new CircleCollider());
            sprite.play(Animation.WalkUp);
            */





            // set the scene so Nez can take over
            Core.scene = myScene;
        }
    }
}
