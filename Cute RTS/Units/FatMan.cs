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
        public override int MoveSpeed { get; set; } = 7;

        public FatMan(TextureAtlas texture, TiledMap tmc, string collisionlayer) : 
            base(texture, tmc, collisionlayer)
        {
            transform.setScale(new Vector2(0.2f, 0.2f));
        }
    }
}
