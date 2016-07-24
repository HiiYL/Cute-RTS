using Microsoft.Xna.Framework.Graphics;
using Nez.TextureAtlases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Units
{
    class FatMan : BaseUnit
    {
        public FatMan(TextureAtlas texture) : base(texture)
        {
            Health = 50;
            Damage = 20;
            Vision = 10;


        }
    }
}
