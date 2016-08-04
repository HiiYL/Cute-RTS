using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Cute_RTS.Components;
using System;

namespace Cute_RTS.AI
{
    class SteeringBehaviours
    {
        // Weights for the various steering forces.  Change these to modifiy behaviour
        private const int SeparationWeight = 35;
        private const int CohesionWeight = 1;
        private const int AlignmentWeight = 6;

        private readonly FlockingComponent flockingComponentContext;
        private readonly float maximumSpeed;
        
        public SteeringBehaviours(float maxSpeed, FlockingComponent flockingComponent)
        {
            flockingComponentContext = flockingComponent;
            maximumSpeed = maxSpeed;
        }

        public Vector2 CalculateFlocking(List<FlockingComponent> flockingComponents)
        {
            Vector2 separationForce = Vector2.Zero;  // Force pushing agents apart
            Vector2 centerOfMass = Vector2.Zero;
            Vector2 cohesionForce = Vector2.Zero; // Force to keep agents together as a group
            Vector2 alignmentForce = Vector2.Zero; // Force to align agent headings

            float neighbourCount = 0;

            // Loop all agents
            foreach (FlockingComponent flockingComponent in flockingComponents)
            {
                // Don't check an agent against itself
                if (flockingComponent != flockingComponentContext)
                {
                    // Calculate distance between agents
                    Vector2 separation = flockingComponentContext.entity.transform.position - flockingComponent.entity.transform.position;
                    //Console.WriteLine(separation);
                    float distance = separation.Length();
                    

                    // If agent is within specified sensor distance...
                    if (distance < flockingComponentContext.SensorDistance)
                    {
                        alignmentForce += flockingComponent.Velocity;
                        centerOfMass += flockingComponent.entity.transform.position;
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
            //Console.WriteLine("HELLO THERE!");
            //Console.WriteLine(separationForce);
            //+(cohesionForce * CohesionWeight) + (alignmentForce * AlignmentWeight)

            return (separationForce * SeparationWeight) + (cohesionForce * CohesionWeight) + (alignmentForce * AlignmentWeight);
        }

        // Steering behaviour to move agents towards a target
        public Vector2 Seek(Vector2 target)
        {
            Console.WriteLine("SEEKING!");
            Vector2 desiredVelocity = Vector2.Normalize(target - flockingComponentContext.entity.transform.position) * maximumSpeed;
            return (desiredVelocity - flockingComponentContext.Velocity);
        }

        public Vector2 ClampVelocity(Vector2 velocity)
        {
            if (velocity.Length() > maximumSpeed)
                velocity = Vector2.Normalize(velocity) * maximumSpeed;

            return velocity;
        }
    }
}
