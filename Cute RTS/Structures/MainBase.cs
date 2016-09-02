using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Structures
{
    class MainBase : Attackable
    {
        private Selectable _selectable;
        private Sprite<Animation> _sprite;
        private BoxCollider _collider;
        private Animation _animation;

        public Selectable Select { get { return _selectable; } }
        public enum Animation
        {
            None,
            Default,
            BuildingUnit
        }

        public MainBase(TiledMap tmc, TextureAtlas atlas, Texture2D selectTex, Player player) :
            base(tmc)
        {
            UnitPlayer = player;
            transform.scale = new Vector2(0.7f, 0.7f);

            _selectable = new Selectable(new Sprite(selectTex));
            addComponent(_selectable);
            
            _sprite = new Sprite<Animation>();
            _sprite.color = UnitPlayer.PlayerColor;
            setupAnimation(atlas);
            addComponent(_sprite);

            _collider = new BoxCollider(-17, -35, 35, 70);
            Flags.setFlagExclusive(ref _collider.physicsLayer, (int)RTSCollisionLayer.Units);
            colliders.add(_collider);

            OnUnitDied += MainBase_OnUnitDied;
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

        private void setupAnimation(TextureAtlas atlas)
        {
            _sprite.addAnimation(Animation.Default, atlas.getSpriteAnimation("idle"));
            _sprite.addAnimation(Animation.BuildingUnit, atlas.getSpriteAnimation("training"));
        }

        private void MainBase_OnUnitDied(Attackable idied)
        {
            throw new NotImplementedException();
        }
    }
}
