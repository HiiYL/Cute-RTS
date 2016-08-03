using Flocking;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS
{
    #region FlockingAIParameters
    public struct AIParameters
    {
        /// <summary>
        /// how far away the animals see each other
        /// </summary>
        public float DetectionDistance;
        /// <summary>
        /// seperate from animals inside this distance
        /// </summary>
        public float SeparationDistance;
        /// <summary>
        /// how much the animal tends to move in it's previous direction
        /// </summary>
        public float MoveInOldDirectionInfluence;
        /// <summary>
        /// how much the animal tends to move with animals in it's detection distance
        /// </summary>
        public float MoveInFlockDirectionInfluence;
        /// <summary>
        /// how much the animal tends to move randomly
        /// </summary>
        public float MoveInRandomDirectionInfluence;
        /// <summary>
        /// how quickly the animal can turn
        /// </summary>
        public float MaxTurnRadians;
        /// <summary>
        /// how much each nearby animal influences it's behavior
        /// </summary>
        public float PerMemberWeight;
        /// <summary>
        /// how much dangerous animals influence it's behavior
        /// </summary>
        public float PerDangerWeight;
    }
    #endregion
    class GroupOfAngryMen : Entity
    {

        // Default value for the AI parameters
        const float detectionDefault = 70.0f;
        const float separationDefault = 50.0f;
        const float moveInOldDirInfluenceDefault = 1.0f;
        const float moveInFlockDirInfluenceDefault = 1.0f;
        const float moveInRandomDirInfluenceDefault = 0.05f;
        const float maxTurnRadiansDefault = 6.0f;
        const float perMemberWeightDefault = 1.0f;
        const float perDangerWeightDefault = 50.0f;


        Flock flock;

        AIParameters flockParams;

        public GroupOfAngryMen()
        {
            flock = null;

            flockParams = new AIParameters();
            ResetAIParams();
        }

        private void ResetAIParams()
        {
            flockParams.DetectionDistance = detectionDefault;
            flockParams.SeparationDistance = separationDefault;
            flockParams.MoveInOldDirectionInfluence = moveInOldDirInfluenceDefault;
            flockParams.MoveInFlockDirectionInfluence = moveInFlockDirInfluenceDefault;
            flockParams.MoveInRandomDirectionInfluence = moveInRandomDirInfluenceDefault;
            flockParams.MaxTurnRadians = maxTurnRadiansDefault;
            flockParams.PerMemberWeight = perMemberWeightDefault;
            flockParams.PerDangerWeight = perDangerWeightDefault;
        }

    }
}
