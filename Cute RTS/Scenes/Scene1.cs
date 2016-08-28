using Cute_RTS.AI;
using Cute_RTS.Structures;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.TextureAtlases;
using Nez.Tiled;
using Nez.UI;
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

        public Scene1():base()
        {
            Console.WriteLine("Init after");
        }

        public override void initialize()
        {
            base.initialize();
            
            Console.WriteLine("Init before");
            var targettex = content.Load<Texture2D>("target");
            Selector.getSelector().TargetTex = targettex;
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

            Func<BaseUnit> giveMeCat = delegate
            {
                var mys = new BaseUnit(catTexture, catSelection, myself, tiledmap, "Stuff");
                mys.setSelectionColor(Color.LawnGreen);
                return addEntity(mys);
            };

            Func<BaseUnit> giveEnemyCat = delegate
            {
                var enem = new BaseUnit(catTexture, catSelection, enemy, tiledmap, "Stuff");
                enem.setSelectionColor(Color.Red);
                return addEntity(enem);
            };

            Func<BaseUnit> giveEnemyCatStructure = delegate
            {
                var enem = new BaseStructure(catTexture, catSelection, enemy, tiledmap, "Stuff");
                enem.setSelectionColor(Color.Red);
                return addEntity(enem);
            };

            //var bar = new ProgressBar(0, 1, 0.1f, false, ProgressBarStyle.create(Color.Black, Color.White));

            BaseUnit kitty = giveMeCat();
            kitty.transform.position = new Vector2(0, 500);
            kitty.gotoLocation(new Point(300, 500));

            kitty = giveMeCat();
            kitty.transform.position = new Vector2(0, 100);
            kitty.gotoLocation(new Point(300, 100));

            kitty = giveMeCat();
            kitty.transform.position = new Vector2(0, 400);
            kitty.gotoLocation(new Point(300, 400));

            Selector.getSelector().ActivePlayer = myself;
            var selectionComponent = tiledEntity.addComponent(Selector.getSelector());
            selectionComponent.renderLayer = -5;


            kitty = giveMeCat();
            kitty.transform.position = new Vector2(0, 400);
            kitty.gotoLocation(new Point(300, 400));
            kitty.MoveSpeed = 0;
            kitty.Range = 15f;

            var enemyCat = giveEnemyCat();
            enemyCat.transform.position = new Vector2(700, 500);
            enemyCat.gotoLocation(new Point(400, 450));

            //var enemyCatStructure = giveEnemyCatStructure();
            //enemyCatStructure.transform.position = new Vector2(700, 500);
            //enemyCatStructure.MoveSpeed = 0;
        }
    }
}
