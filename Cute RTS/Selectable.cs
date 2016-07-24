using Nez;
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
                    Selector.getSelector().add(this);
                }
                else
                {
                    Selector.getSelector().remove(this);
                }
            }
        }
        
    }
}
