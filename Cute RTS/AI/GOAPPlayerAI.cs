using Nez;
using Nez.AI.GOAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.AI
{
    class GOAPPlayerAI : Component, IUpdatable
    {
        private ActionPlanner _planner;

        public class PlayerState
        {
            public enum Location
            {
                InTransit,
                Bank,
                Mine,
                Home,
                Saloon
            }

            public const int MAX_FATIGUE = 10;
            public const int MAX_GOLD = 8;
            public const int MAX_THIRST = 5;

            public int fatigue;
            public int thirst;
            public int gold;
            public int goldInBank;

            public Location currentLocation = Location.Home;
        }

        const string IS_FATIGUED = "fatigued";
        const string IS_THIRSTY = "thirsty";
        const string HAS_ENOUGH_GOLD = "hasenoughgold";

        public PlayerState playerState = new PlayerState();

        public GOAPPlayerAI()
        {
            _planner = new ActionPlanner();
        }
        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
        }

        WorldState getGoalState()
        {
            var goalState = _planner.createWorldState();

            if (playerState.fatigue >= PlayerState.MAX_FATIGUE)
                goalState.set(IS_FATIGUED, false);
            else if (playerState.thirst >= PlayerState.MAX_THIRST)
                goalState.set(IS_THIRSTY, false);
            else if (playerState.gold >= PlayerState.MAX_GOLD)
                goalState.set(HAS_ENOUGH_GOLD, false);
            else
                goalState.set(HAS_ENOUGH_GOLD, true);

            return goalState;
        }
        public void update()
        {
            entity.updateInterval = 10;
            //throw new NotImplementedException();
        }
    }
}
