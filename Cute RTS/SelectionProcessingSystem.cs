using Cute_RTS.Units;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS
{
    class SelectionProcessingSystem : ProcessingSystem
    {
        public override void process()
        {
            if (Input.leftMouseButtonReleased)
            {
                Collider v = Physics.overlapRectangle(new RectangleF(Input.mousePosition.X, Input.mousePosition.Y, 5, 5));
                if (v != null)
                {
                    var humanFootman = v.entity.getComponent<HumanFootman>();
                    if (humanFootman != null)
                    {
                            humanFootman.interactable = !humanFootman.interactable;
                    }
                }
            }
        }
    }
}
