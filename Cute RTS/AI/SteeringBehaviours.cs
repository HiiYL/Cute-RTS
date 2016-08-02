using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Cute_RTS.AI
{
    public class SteeringBehaviours
    {
        // Weights for the various steering forces.  Change these to modifiy behaviour
        private const int SeparationWeight = 35;
        private const int CohesionWeight = 1;
        private const int AlignmentWeight = 6;

        private readonly Agent agentContext;
        private readonly float maximumSpeed;
        
        public SteeringBehaviours(Agent agent, float maxSpeed)
        {
            agentContext = agent;
            maximumSpeed = maxSpeed;
        }

        public Vector2 CalculateFlocking(List<Agent> agents)
        {
            Vector2 separationForce = Vector2.Zero;  // Force pushing agents apart
            Vector2 centerOfMass = Vector2.Zero;
            Vector2 cohesionForce = Vector2.Zero; // Force to keep agents together as a group
            Vector2 alignmentForce = Vector2.Zero; // Force to align agent headings

            float neighbourCount = 0;

            // Loop all agents
            foreach (Agent agent in agents)
            {
                // Don't check an agent against itself
                if (agent != agentContext)
                {
                    // Calculate distance between agents
                    Vector2 separation = agentContext.Entity.transform.position - agent.Entity.transform.position;
                    float distance = separation.Length();

                    // If agent is within specified sensor distance...
                    if (distance < agentContext.SensorDistance)
                    {
                        alignmentForce += agent.Velocity;
                        centerOfMass += agent.Entity.transform.position;
                        separationForce += Vector2.Normalize(separation) / distance;

                        neighbourCount++;
                    }
                }
            }

            // If agent has neighbours then calculate average alignment and center of mass
            if (neighbourCount > 0)
            {
                alignmentForce /= neighbourCount;

                centerOfMass /= neighbourCount;
                cohesionForce = Seek(centerOfMass);
            }

            return (separationForce * SeparationWeight) + (cohesionForce * CohesionWeight) + (alignmentForce * AlignmentWeight);
        }

        // Steering behaviour to move agents towards a target
        public Vector2 Seek(Vector2 target)
        {
            Vector2 desiredVelocity = Vector2.Normalize(target - agentContext.Entity.transform.position) * maximumSpeed;
            return (desiredVelocity - agentContext.Velocity);
        }

        public Vector2 ClampVelocity(Vector2 velocity)
        {
            if (velocity.Length() > maximumSpeed)
                velocity = Vector2.Normalize(velocity) * maximumSpeed;

            return velocity;
        }
    }
}
