using Cute_RTS.AI;
using Cute_RTS.Components;
using Cute_RTS.Systems;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.TextureAtlases;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Scenes
{
    class Scene1 : BaseScene
    {
        private const int PopulationSize = 175;


        public Scene1(bool addExcludeRenderer = true, bool needsFullRenderSizeForUI = false) : base(addExcludeRenderer, needsFullRenderSizeForUI)
        {
        }

        public override void initialize()
        {

            var tiledEntity = createEntity("tiled-map-entity");
            var tiledmap = content.Load<TiledMap>("cute-map");
            var tmc = new TiledMapComponent(tiledmap, "Stuff");

            var tiledMapComponent = tiledEntity.addComponent(tmc);
            Flags.setFlagExclusive(ref tiledMapComponent.physicsLayer, (int)RTSCollisionLayer.Map);
            tiledMapComponent.renderLayer = 10;

            //TextureAtlas baldyTexture = content.Load<TextureAtlas>("BaldyAtlas");
            TextureAtlas baldyTexture = content.Load<TextureAtlas>("CatAtlas");
            Texture2D catSelection = content.Load<Texture2D>("Units/Cat/cat-selection");

            float angle = MathHelper.ToRadians(270);

            Cat fatman = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            addEntity(fatman);
            fatman.transform.position = new Vector2(0, 500);
            fatman.gotoLocation(new Point(300, 500));



            Cat fatman2 = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            addEntity(fatman2);
            fatman2.transform.position = new Vector2(0, 100);
            fatman2.gotoLocation(new Point(300, 500));


            Cat fatman3 = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            addEntity(fatman3);
            fatman3.transform.position = new Vector2(0, 400);
            fatman3.gotoLocation(new Point(300, 500));

            var selectionComponent = tiledEntity.addComponent(Selector.getSelector());
            selectionComponent.renderLayer = -5;

            Cat fatman4 = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            addEntity(fatman4);
            fatman4.transform.position = new Vector2(0, 400);
            fatman4.gotoLocation(new Point(300, 500));


            addEntityProcessor(new FlockingSystem(new Matcher().all(typeof(FlockingComponent))));

        }
    }
}
