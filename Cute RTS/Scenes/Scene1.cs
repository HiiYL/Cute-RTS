﻿using Cute_RTS.AI;
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
            var tiledmap = content.Load<TiledMap>("map");
            var tmc = new TiledMapComponent(tiledmap, "collision");

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

            Selector.getSelector().ActivePlayer = myself;
            var selectionComponent = tiledEntity.addComponent(Selector.getSelector());
            selectionComponent.renderLayer = -5;

            Func<BaseUnit> giveMeCat = delegate
            {
                var mys = new BaseUnit(catTexture, catSelection, myself, tiledmap, "collision");
                mys.Select.setSelectionColor(Color.LawnGreen);
                return addEntity(mys);
            };

            Func<BaseUnit> giveEnemyCat = delegate
            {
                var enem = new BaseUnit(catTexture, catSelection, enemy, tiledmap, "collision");
                enem.Select.setSelectionColor(Color.Red);
                return addEntity(enem);
            };

            Func<BaseUnit> giveEnemyCatStructure = delegate
            {
                var enem = new BaseStructure(catTexture, catSelection, enemy, tiledmap, "collision");
                enem.Select.setSelectionColor(Color.Red);
                return addEntity(enem);
            };

            List<TiledObject> flags = tiledmap.getObjectGroup("objects").objectsWithName("Flag");
            var flagTexture = content.Load<Texture2D>("flag");
            var flagSelectionTexture = content.Load<Texture2D>("flag-selection");
            CaptureFlag captureflag;
            foreach (TiledObject f in flags)
            {
                captureflag = new CaptureFlag(flagTexture, flagSelectionTexture);
                captureflag.transform.position = new Vector2(f.x, f.y);
                addEntity(captureflag);
            }

            BaseUnit kitty = giveMeCat();
            kitty.transform.position = new Vector2(100, 200);
            kitty = giveMeCat();
            kitty.FullHealth = 150;
            kitty.transform.position = new Vector2(150, 230);

            var enemyCat = giveEnemyCat();
            enemyCat.transform.position = new Vector2(700, 100);

            var enemyCatStructure = giveEnemyCatStructure();
            enemyCatStructure.transform.position = new Vector2(700, 150);
            enemyCatStructure.MoveSpeed = 0;
        }
    }
}
