using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.TextureAtlases;
using Nez.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Units
{
    class Cat : BaseUnit
    {
        public override int MoveSpeed { get; set; } = 7;

        public Cat(TextureAtlas texture, Texture2D selectTex, TiledMap tmc, string collisionlayer) : 
            base(texture, selectTex, tmc, collisionlayer)
        {
            transform.setScale(new Vector2(0.5f, 0.5f));
        }
    }
}
