using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class BehaviorGraph
    {
        private readonly BrowserObjects _browserObjects;
        private readonly List<Behavior> _behaviors = new List<Behavior>();

        internal BehaviorGraph(BrowserObjects browserObjects)
        {
            _browserObjects = browserObjects;
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
                _browserObjects.DeclareEvent(
                    componentEvent.ComponentKey,
                    ObjectEventContract.ForComponentEvent(componentEvent.EventName));
            }
        }
    }
}
