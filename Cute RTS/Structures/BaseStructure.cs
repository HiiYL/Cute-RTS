using Cute_RTS.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Nez.TextureAtlases;
using Nez.Tiled;

namespace Cute_RTS.Structures
{
    class BaseStructure : BaseUnit
    {
        public BaseStructure(TextureAtlas atlas, Texture2D selectTex, Player player, TiledMap tmc, string collisionlayer) : base(atlas, selectTex, player, tmc, collisionlayer)
        {
        }

        public override int Damage { get; set; } = 10;
        public override float Range { get; set; } = 20f; // default is melee
        public override int Vision { get; set; } = 15;
        public override int MoveSpeed { get; set; } = 0;
        public override float AttackSpeed { get; set; } = 1.5f; // in seconds
    }
}
