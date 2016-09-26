using System;
using Nez.AI.UtilityAI;
using Nez;

namespace Cute_RTS.AI
{
    class UtilityPlayerAI : Component, IUpdatable
    {
        UtilityAI<UtilityPlayerAI> _ai;

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();

        }

        void IUpdatable.update()
        {
            //_ai.tick();
        }
    }
}
