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
    class FatMan : BaseUnit
    {
        public FatMan(TextureAtlas texture, TiledMap tmc, string collisionlayer) : 
            base(texture, tmc, collisionlayer)
        {
            Health = 50;
            Damage = 20;
            Vision = 10;
            transform.setScale(new Vector2(0.4f, 0.4f));
        }
    }
}
