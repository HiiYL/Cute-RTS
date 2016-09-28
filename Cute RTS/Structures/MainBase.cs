using Cute_RTS.Components;
using Cute_RTS.Scenes;
using Cute_RTS.Units;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Tiled;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Cute_RTS.Structures
{
    class MainBase : Attackable
    {
        private Sprite<Animation> _sprite;
        private BoxCollider _collider;
        private Animation _animation;

        private int _UPDATE_INTERVAL = 50;
        private int _TRAIN_TIME = 2000;

        private int _UPDATE_COUNT;
        private int _CURRENT_UPDATE_COUNT = 0;

        private Player _player;

        private ProgressBar _trainingBar;
        private UnitRadar _radar;


        private Timer _trainTimer;
        private float _TRAINBAR_WIDTH = 100;
        private float _TRAINBAR_HEIGHT = 20;
        private Text _displayText;

        public enum Animation
        {
            None,
            Default,
            BuildingUnit,
            Explode
        }

        public MainBase(TiledMap tmc, TextureAtlas atlas, Texture2D selectTex, Player player) :
            base(tmc, new Sprite(selectTex), player)
        {

            _UPDATE_COUNT = _TRAIN_TIME / _UPDATE_INTERVAL;
            FullHealth = 100;
            transform.scale = new Vector2(0.7f, 0.7f);

            var h = new HealthBar(this);
            h.PositionOffset = new Vector2(30, 55);
            addComponent(h);

            _sprite = new Sprite<Animation>();
            _sprite.color = UnitPlayer.PlayerColor;
            setupAnimation(atlas);
            addComponent(_sprite);

            _radar = new UnitRadar(200);
            addComponent(_radar);

            _collider = new BoxCollider(-17, -35, 35, 70);
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)RTSCollisionLayer.Units);
            colliders.add(_collider);

            OnUnitDied += MainBase_OnUnitDied;

            _trainTimer = new Timer(_UPDATE_INTERVAL);
            _trainTimer.Elapsed += _trainTimer_Elapsed;

            _displayText = new Text(Graphics.instance.bitmapFont, "", new Vector2(5, -40), Color.LightGoldenrodYellow);
            //_displayText.setText("DIE DIE DIE!");
            _displayText.setRenderLayer(-10);

            addComponent(_displayText);

            _player = player;
        }

        public List<Attackable> getSurroundingEnemies()
        {
            if (!isAlive) return new List<Attackable>();

            return _radar.getEnemiesInArea();
        }

        public override void onAddedToScene()
        {
            playAnimation(Animation.Default);
            base.onAddedToScene();
        }

        public void playAnimation(Animation anim)
        {
            if (anim != Animation.None && _animation != anim)
            {
                _animation = anim;
                _sprite.play(_animation);
            }
        }

        public void onTrainUnitBtnPressed(Button button)
        {
            trainUnit();

        }
        public void trainUnit()
        {
            if (!isAlive) return;

            if(_trainingBar != null)
            {
                _trainingBar.setVisible(true);

            }else
            {
                var knobBefore = new PrimitiveDrawable(Color.Green);
                knobBefore.minWidth = _TRAINBAR_WIDTH;
                knobBefore.minHeight = _TRAINBAR_HEIGHT;
                var knobAfter = new PrimitiveDrawable(Color.Red);
                knobAfter.minWidth = _TRAINBAR_WIDTH;
                knobAfter.minHeight = _TRAINBAR_HEIGHT;
                _trainingBar = new ProgressBar(0, 1, 0.05f, false, new ProgressBarStyle
                {
                    knobBefore = knobBefore,
                    knobAfter = knobAfter
                });
            }
           
            ((GameScene)scene)._selectedUnitTable.add(_trainingBar);
            if (UnitPlayer.Gold >= 50 && !_trainTimer.Enabled)
            {
                _trainTimer.Start();
                _sprite.play(Animation.BuildingUnit);
                UnitPlayer.Gold -= 50;
            }
        }
        private void setupAnimation(TextureAtlas atlas)
        {
            _sprite.addAnimation(Animation.Default, atlas.getSpriteAnimation("idle"));
            _sprite.addAnimation(Animation.BuildingUnit, atlas.getSpriteAnimation("training"));
            var explo = atlas.getSpriteAnimation("explosion");
            explo.loop = false;
            _sprite.addAnimation(Animation.Explode, explo);
        }

        private void MainBase_OnUnitDied(Attackable idied)
        {
            Scene s = scene;
            _sprite.color = Color.White;
            playAnimation(Animation.Explode);
            Core.schedule(1f, timer =>
            {
                destroy();
            });
        }


        private void _trainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!isAlive) return;

            if(_CURRENT_UPDATE_COUNT > _UPDATE_COUNT)
            {
                _sprite.play(Animation.Default);
                TextureAtlas catTexture = scene.content.Load<TextureAtlas>("CatAtlas");
                Texture2D catSelection = scene.content.Load<Texture2D>("Units/Cat/cat-selection");
                var enem = new BaseUnit(catTexture, catSelection, UnitPlayer, UnitTileMap, "collision");
                
                enem.transform.position = transform.position + new Vector2(0, 64);
                if (Selector.getSelector().ActivePlayer == UnitPlayer)
                {
                    enem.Select.setSelectionColor(Color.LawnGreen);
                } else
                {
                    enem.Select.setSelectionColor(Color.Red);
                }
                
                _trainTimer.Stop();
                scene.addEntity(enem);
                _CURRENT_UPDATE_COUNT = 0;
                _trainingBar.setVisible(false);
            }
            else
            {
                Console.WriteLine(_CURRENT_UPDATE_COUNT / (float)_UPDATE_COUNT);
                _CURRENT_UPDATE_COUNT++;
                var progress = _CURRENT_UPDATE_COUNT / (float)_UPDATE_COUNT;
                _trainingBar.setValue(progress);

                _displayText.setText(String.Format("Training : {0:P0}", progress));

            }

        }
    }
}
