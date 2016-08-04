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

            TextureAtlas catTexture = content.Load<TextureAtlas>("CatAtlas");
            Texture2D catSelection = content.Load<Texture2D>("Units/Cat/cat-selection");

            Vector2 velocity = new Vector2(1 * MaxSpeed, 0);

            float angle = MathHelper.ToRadians(270);

            Player myself = new Player(Color.Aqua, "Robert Baratheon");
            Player enemy = new Player(Color.Orchid, "Enemy AI");
            myself.Opponent = enemy;
            enemy.Opponent = myself;

            Func<BaseUnit> giveMeCat = delegate {
                var mys = new BaseUnit(catTexture, catSelection, myself, tiledmap, "Stuff");
                mys.setSelectionColor(Color.LawnGreen);
                return addEntity(mys);
            };

            Func<BaseUnit> giveEnemyCat = delegate {
                var enem = new BaseUnit(catTexture, catSelection, enemy, tiledmap, "Stuff");
                enem.setSelectionColor(Color.Red);
                return addEntity(enem);
            };

            BaseUnit kitty = giveMeCat();
            kitty.transform.position = new Vector2(0, 500);
            kitty.gotoLocation(new Point(300, 500));

            Agent agent = new Agent(kitty, SensorDistance, MaxSpeed);
            agent.Velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angle));
            agents.Add(agent);

            kitty = giveMeCat();
            kitty.transform.position = new Vector2(0, 100);
            kitty.gotoLocation(new Point(300, 100));

            Agent agent2 = new Agent(kitty, SensorDistance, MaxSpeed);
            agent2.Velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angle));
            agents.Add(agent2);

            kitty = giveMeCat();
            kitty.transform.position = new Vector2(0, 400);
            kitty.gotoLocation(new Point(300, 400));

            Agent agent3 = new Agent(kitty, SensorDistance, MaxSpeed);
            agent3.Velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angle));
            agents.Add(agent3);

            Selector.getSelector().ActivePlayer = myself;
            var selectionComponent = tiledEntity.addComponent(Selector.getSelector());
            selectionComponent.renderLayer = -5;

            kitty = giveMeCat();
            kitty.transform.position = new Vector2(0, 400);
            kitty.gotoLocation(new Point(300, 400));

            Agent agent4 = new Agent(kitty, SensorDistance, MaxSpeed);
            agent4.Velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angle));
            agents.Add(agent4);

            var enemyCat = giveEnemyCat();
            enemyCat.transform.position = new Vector2(500, 500);
            enemyCat.gotoLocation(new Point(400, 450));
        }
    }
}
