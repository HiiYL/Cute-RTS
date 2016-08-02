using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Sprites;
using Nez;

namespace Cute_RTS.AI
{
    public class Agent
    {
        private const int NumSamplesForSmoothing = 5;

        private readonly HeadingSmoother smoother;
        private readonly SteeringBehaviours steering;

        private Vector2 forward = new Vector2(1, 0);
        private Vector2 side = new Vector2(0, 1);

        public Agent(Entity entity, int sensorDistance, float maxSpeed)
        {
            SensorDistance = sensorDistance;

            Entity = entity;

            smoother = new HeadingSmoother(NumSamplesForSmoothing);
            steering = new SteeringBehaviours(this, maxSpeed);
        }

        public void Update(List<Agent> agents)
        {
            Velocity = steering.CalculateFlocking(agents);
            smoother.InsertSample(Velocity);

            Velocity = steering.ClampVelocity(Velocity);
            Entity.transform.position += Velocity;
            CalculateHeading();
        }       

        private void CalculateHeading()
        {
            forward = smoother.CalculateSmoothedHeading();
            Entity.transform.rotation = (float)Math.Atan2(forward.Y, forward.X);

            side = Vector2.Transform(forward, Matrix.CreateRotationZ(MathHelper.PiOver2));
            side.Normalize();
        }

        public Vector2 Velocity { get; set; }
        public Entity Entity { get; private set; }
        public int SensorDistance { get; private set; }

        public Vector2 Forward
        {
            get { return forward; }
        }

        public Vector2 Side
        {
            get { return side; }
        }
    }
}
