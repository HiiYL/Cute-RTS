using Cute_RTS.AI;
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
        private const int SensorDistance = 50;
        private const float MaxSpeed = 2f;

        private readonly List<Agent> agents = new List<Agent>();

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

            Vector2 velocity = new Vector2(1 * MaxSpeed, 0);

            float angle = MathHelper.ToRadians(270);


            Cat fatman = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            addEntity(fatman);
            fatman.transform.position = new Vector2(0, 500);
            fatman.gotoLocation(new Point(300, 500));

            Agent agent = new Agent(fatman, SensorDistance, MaxSpeed);
            agent.Velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angle));
            agents.Add(agent);

            Cat fatman2 = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            addEntity(fatman2);
            fatman2.transform.position = new Vector2(0, 100);
            fatman2.gotoLocation(new Point(300, 100));

            Agent agent2 = new Agent(fatman2, SensorDistance, MaxSpeed);
            agent2.Velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angle));
            agents.Add(agent2);


            Cat fatman3 = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            addEntity(fatman3);
            fatman3.transform.position = new Vector2(0, 400);
            fatman3.gotoLocation(new Point(300, 400));

            Agent agent3 = new Agent(fatman3, SensorDistance, MaxSpeed);
            agent3.Velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angle));
            agents.Add(agent3);

            var selectionComponent = tiledEntity.addComponent(Selector.getSelector());
            selectionComponent.renderLayer = -5;

            Cat fatman4 = new Cat(baldyTexture, catSelection, tiledmap, "Stuff");
            addEntity(fatman4);
            fatman4.transform.position = new Vector2(0, 400);
            fatman4.gotoLocation(new Point(300, 400));

            Agent agent4 = new Agent(fatman4, SensorDistance, MaxSpeed);
            agent4.Velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angle));
            agents.Add(agent4);
        }
    }
}
