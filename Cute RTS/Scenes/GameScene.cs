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
    class GameScene : BaseScene
    {
        private const int PopulationSize = 175;
        private const int SensorDistance = 50;
        private const float MaxSpeed = 2f;

        private Table _resourceTable;

        public Table _selectedUnitTable { get; set; }

    private readonly List<Agent> agents = new List<Agent>();
        public ImageButton _attackBtn { get; set; }
        public ImageButton _stopBtn { get; set; }


        public List<CaptureFlag> captureFlags;

        public bool isAttackBtnClicked = false;
        private Player myself;
        private Player enemy;
        private ImageButton _catBtn;
        private ProgressBar _unitTrainingBar;

        public GameScene():base()
        {
            Console.WriteLine("Init after");
        }

        public override void initialize()
        {
            base.initialize();


            _resourceTable = canvas.stage.addElement(new Table());
            _resourceTable.setFillParent(true).top().left();

            var coinTex = content.Load<Texture2D>("coin");
            var goldLbl = new Label("Gold: 0");
            goldLbl.setFontColor(Color.Black);
            goldLbl.setFontScale(2f);

            _resourceTable.add(new Image(coinTex));
            _resourceTable.add(goldLbl);


            _resourceTable = canvas.stage.addElement(new Table());
            _resourceTable.setFillParent(true).top().left();


            _selectedUnitTable = canvas.stage.addElement(new Table());

            _selectedUnitTable.setFillParent(true).bottom().left();

            var attackTex = content.Load<Texture2D>("attack");
            _attackBtn = new ImageButton(new ImageButtonStyle(new PrimitiveDrawable(Color.Red), new PrimitiveDrawable(Color.Black), new PrimitiveDrawable(Color.Blue),
                new SubtextureDrawable(attackTex), new SubtextureDrawable(attackTex), new SubtextureDrawable(attackTex)));
            _selectedUnitTable.add(_attackBtn).setMinWidth(100).setMinHeight(30);


            var stopTex = content.Load<Texture2D>("stop");

            _stopBtn = new ImageButton(new ImageButtonStyle(new PrimitiveDrawable(Color.Red), new PrimitiveDrawable(Color.Black), new PrimitiveDrawable(Color.Blue),
    new SubtextureDrawable(stopTex), new SubtextureDrawable(stopTex), new SubtextureDrawable(stopTex)));
            _selectedUnitTable.add(_stopBtn).setMinWidth(100).setMinHeight(30);


            /*
            var catTex = content.Load<Texture2D>("train-cat");

            _catBtn = new ImageButton(new ImageButtonStyle(new PrimitiveDrawable(Color.Red), new PrimitiveDrawable(Color.Black), new PrimitiveDrawable(Color.Blue),
    new SubtextureDrawable(catTex), new SubtextureDrawable(catTex), new SubtextureDrawable(catTex)));
            _selectedUnitTable.add(_catBtn).setMinWidth(100).setMinHeight(30);
            */

            //_unitTrainingBar = new ProgressBar(0, 1, 0.05f, false, ProgressBarStyle.create(Color.Green, Color.Red));
            //_selectedUnitTable.add(_unitTrainingBar);

            _selectedUnitTable.setVisible(false);

            Console.WriteLine("Init before");
            var targettex = content.Load<Texture2D>("target");
            Selector.getSelector().TargetTex = targettex;

            _stopBtn.onClicked += Selector.getSelector().onStopMovingBtnPressed;
            _attackBtn.onClicked += Selector.getSelector().onAttackBtnPressed;



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

            myself = new Player(Color.Aqua, "Robert Baratheon");
            myself.OnGoldChange += delegate(int amount)
            {
                goldLbl.setText("Gold: " + amount.ToString());
            };
            enemy = new AIPlayer(Color.Orchid, "Enemy AI", myself);
            myself.Opponent = enemy;
            //enemy.Opponent = myself;

            addEntity(enemy);

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

            List<TiledObject> startlocations = tiledmap.getObjectGroup("objects").objectsWithName("StartLocation");
            TextureAtlas mainBaseAtlas = content.Load<TextureAtlas>("MainBaseAtlas");
            Texture2D mainBaseSelection = content.Load<Texture2D>("Structures/MainBase/MainBase-selection");
            Func<TiledObject, Player, MainBase> makeBase = delegate(TiledObject obj, Player pl)
            {
                MainBase playerbase = new MainBase(tiledmap, mainBaseAtlas, mainBaseSelection, pl);
                // place base at center of elipse marker
                playerbase.transform.position = new Vector2(obj.x + obj.width / 2, obj.y + obj.height / 2);
                if (pl == myself)
                {
                    playerbase.Select.setSelectionColor(Color.LawnGreen);
                } else
                {
                    playerbase.Select.setSelectionColor(Color.Red);
                }
                pl.mainBase = playerbase;
                
                return addEntity(playerbase);
            };

            makeBase(startlocations.ElementAt(0), myself);
            makeBase(startlocations.ElementAt(1), enemy);

            List <TiledObject> flags = tiledmap.getObjectGroup("objects").objectsWithName("Flag");
            var flagTexture = content.Load<Texture2D>("flag");
            var flagSelectionTexture = content.Load<Texture2D>("flag-selection");
            CaptureFlag captureflag;
            captureFlags = new List<CaptureFlag>();

            int tileSize = 16;
            //int nearestMultiple = (int)Math.Round((value / (double)tileSize),MidpointRounding.AwayFromZero) * tileSize;

            foreach (TiledObject f in flags)
            {
                captureflag = new CaptureFlag(flagTexture, flagSelectionTexture);

                int xNearestTile = (int)Math.Round((f.x / (double)tileSize), MidpointRounding.AwayFromZero) * tileSize;
                int yNearestTile = (int)Math.Round((f.y / (double)tileSize), MidpointRounding.AwayFromZero) * tileSize;
                captureflag.transform.position = new Vector2(xNearestTile, yNearestTile);
                Console.WriteLine(xNearestTile);
                Console.WriteLine(yNearestTile);
                Console.WriteLine("");
                captureFlags.Add(captureflag);
                addEntity(captureflag);
            }

            //BaseUnit kitty = giveMeCat();
            //kitty.transform.position = new Vector2(100, 200);


            for (int i = 0; i < 5; i++)
            {
                giveMeCat().transform.position = new Vector2(150, 250);
            }

            for (int i = 0; i < 5; i++)
            {
                giveEnemyCat().transform.position = new Vector2(700, 250);
            }
            /*
            kitty = giveMeCat();
            kitty.FullHealth = 150;
            kitty.transform.position = new Vector2(150, 230);
            */

            /*
            var enemyCat = giveEnemyCat();
            enemyCat.transform.position = new Vector2(700, 200);

            var enemyCat3 = giveEnemyCat();
            enemyCat3.transform.position = new Vector2(700, 250);

            var enemyCat2 = giveEnemyCat();
            enemyCat2.transform.position = new Vector2(750, 200);
            */


            /*
            var enemyCatStructure = giveEnemyCatStructure();
            enemyCatStructure.transform.position = new Vector2(650, 300);
            enemyCatStructure.MoveSpeed = 0;
            */
        }
    }
}
