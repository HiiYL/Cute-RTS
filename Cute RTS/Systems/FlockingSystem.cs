using Cute_RTS.Components;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cute_RTS.Systems
{
    class FlockingSystem : EntityProcessingSystem
    {
        public static int SensorDistance = 50;
        public static float MaxSpeed = 1f;
        List<FlockingComponent> flockingComponents;
        public FlockingSystem(Matcher matcher) : base(matcher)
        {
            flockingComponents = new List<FlockingComponent>();
        }


        public override void process(Entity entity)
        {
            FlockingComponent flockingComponent = entity.getComponent<FlockingComponent>();
            flockingComponent.Update(flockingComponents);
        }
        
        protected override void process(List<Entity> entities)
        {
            flockingComponents.Clear();
            for (var i = 0; i < entities.Count; i++)
                flockingComponents.Add(entities[i].getComponent<FlockingComponent>());

            for (var i = 0; i < entities.Count; i++)
                process(entities[i]);
        }
        
    }
}
