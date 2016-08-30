using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Structures
{
    class CaptureFlag : Entity
    {
        private Sprite _sprite;

        public CaptureFlag(Texture2D tex)
        {
            _sprite = new Sprite(tex);
            // offset to set pitch point of flag as origin
            _sprite.setOrigin(new Vector2(0, 20));
            addComponent(_sprite);
        }

    }
}
