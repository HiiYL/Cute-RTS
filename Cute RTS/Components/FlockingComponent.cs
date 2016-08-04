using Cute_RTS.AI;
using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Components
{
    class FlockingComponent : Component
    {
        private const int NumSamplesForSmoothing = 5;

        private readonly HeadingSmoother smoother;
        private readonly SteeringBehaviours steering;

        private Vector2 forward = new Vector2(1, 0);
        private Vector2 side = new Vector2(0, 1);

        public FlockingComponent(int sensorDistance, float maxSpeed)
        {
            SensorDistance = sensorDistance;

            smoother = new HeadingSmoother(NumSamplesForSmoothing);
            steering = new SteeringBehaviours(maxSpeed, this);
        }

        public void Update(List<FlockingComponent> flockingComponents)
        {
            if (flockingComponents.Count > 1)
            {
                //Console.WriteLine(flockingComponents.Count);
                Velocity = steering.CalculateFlocking(flockingComponents);
                smoother.InsertSample(Velocity);

                Velocity = steering.ClampVelocity(Velocity);
                //Console.WriteLine(Velocity);
                entity.move(Velocity);
                CalculateHeading();
            }
            //entity.transform.position += new Vector2(1);
            
        }

        public void moveTowards(Vector2 position)
        {
            steering.target = position - entity.transform.position;
        }

        private void CalculateHeading()
        {
            forward = smoother.CalculateSmoothedHeading();
            entity.transform.rotation = (float)Math.Atan2(forward.Y, forward.X);
            //Console.WriteLine(forward);

            side = Vector2.Transform(forward, Matrix.CreateRotationZ(MathHelper.PiOver2));
            side.Normalize();
        }

        public Vector2 Velocity { get; set; }
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
