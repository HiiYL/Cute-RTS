using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS
{
    class Selectable : Component
    {
        private bool _isSelected = false;
        private Sprite _selectTex;

        public bool IsSelected {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected == value) return;

                _isSelected = value;
                if (_isSelected)
                {
                    _selectTex.enabled = true;
                    Selector.getSelector().add(this);
                }
                else
                {
                    _selectTex.enabled = false;
                    Selector.getSelector().remove(this);
                }
            }
        }

        public Selectable(Sprite selectTex)
        {
            _selectTex = selectTex;
            _selectTex.enabled = false;
            _selectTex.renderLayer = 1;
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            entity.addComponent(_selectTex);
        }

        public override void onRemovedFromEntity()
        {
            base.onRemovedFromEntity();
            // Remove from Selector
            IsSelected = false;
        }

        public void setSelectionColor(Color color)
        {
            _selectTex.color = color;
        }

        public void playClickSelectAnimation(int counter = 4)
        {
            if (counter == 0) return;

            _selectTex.enabled = !_selectTex.enabled;
            Core.schedule(0.15f, tmr =>
            {
                playClickSelectAnimation(counter - 1);
            });
        }

    }
}
