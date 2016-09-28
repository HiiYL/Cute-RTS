using System;
using Nez.AI.UtilityAI;
using Nez;
using Cute_RTS.Structures;
using Cute_RTS.Units;

namespace Cute_RTS.AI
{
    class UtilityPlayerAI : Component, IUpdatable
    {
        UtilityAI<UtilityPlayerAI> _ai;
        PlayerState _state;
        Player _player;

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            
            _player = entity as Player;
            Assert.isTrue(_player != null);
            _state = new PlayerState(_player);

            var reasoner = new HighestScoreReasoner<UtilityPlayerAI>();


            _ai = new UtilityAI<UtilityPlayerAI>(this, reasoner);
            _ai.updatePeriod = 0.5f;
        }

        private int getThreatLevel()
        {
            var count = _player.mainBase.getSurroundingEnemies().Count;
            return count;
        }

 

        void IUpdatable.update()
        {
            _ai.tick();
        }
    }
}
