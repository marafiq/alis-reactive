using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Immutable plan document — the serialized contract between C# and the browser runtime.
    /// Produced by <see cref="PlanBuildContext.BuildPlan"/> once construction is complete.
    /// </summary>
    internal sealed class Plan
    {
        private readonly PlanIdentity _identity;
        private readonly IReadOnlyDictionary<string, BrowserObjectContract> _types;
        private readonly IReadOnlyDictionary<string, ComponentObject> _components;
        private readonly IReadOnlyList<Behavior> _behaviors;

        public int Version => 3;
        public string PlanId => _identity.PlanIdForJson;
        public PlanScope Scope => _identity.ScopeForJson;
        public IReadOnlyDictionary<string, BrowserObjectContract> Types => _types;
        public IReadOnlyDictionary<string, ComponentObject> Components => _components;
        public IReadOnlyList<Behavior> Behaviors => _behaviors;

        internal Plan(
            PlanIdentity identity,
            IReadOnlyDictionary<string, BrowserObjectContract> types,
            IReadOnlyDictionary<string, ComponentObject> components,
            IReadOnlyList<Behavior> behaviors)
        {
            _identity = identity;
            _types = types;
            _components = components;
            _behaviors = behaviors;
        }
    }
}
