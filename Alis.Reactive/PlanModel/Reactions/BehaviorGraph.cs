using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class BehaviorGraph
    {
        private readonly List<Behavior> _behaviors = new List<Behavior>();

        internal IReadOnlyList<Behavior> Snapshot() => new List<Behavior>(_behaviors);

        internal void Add(Behavior behavior) => _behaviors.Add(behavior);
    }
}
