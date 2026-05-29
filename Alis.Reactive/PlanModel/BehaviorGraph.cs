using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class BehaviorGraph
    {
        private readonly ComponentObjects _components;
        private readonly List<Behavior> _behaviors = new List<Behavior>();

        internal BehaviorGraph(ComponentObjects components)
        {
            _components = components;
        }

        internal IReadOnlyList<Behavior> Snapshot() => new List<Behavior>(_behaviors);

        internal void Add(Behavior behavior)
        {
            RegisterEventMetadataForTrigger(behavior.StartsWhen);
            _behaviors.Add(behavior);
        }

        private void RegisterEventMetadataForTrigger(StartsWhen trigger)
        {
            if (trigger is ComponentEventTrigger componentEvent)
            {
                _components.DeclareEvent(
                    componentEvent.ComponentKey,
                    ObjectEventContract.ForComponentEvent(componentEvent.EventName));
            }
        }
    }
}
