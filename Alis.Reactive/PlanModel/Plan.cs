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
        private readonly IReadOnlyDictionary<string, JsType> _types;
        private readonly IReadOnlyDictionary<string, Component> _components;
        private readonly IReadOnlyList<Behavior> _behaviors;

        public int Version => 3;
        public string PlanId => _identity.PlanIdForJson;
        public PlanScope Scope => _identity.ScopeForJson;
        public IReadOnlyDictionary<string, JsType> Types => _types;
        public IReadOnlyDictionary<string, Component> Components => _components;
        public IReadOnlyList<Behavior> Behaviors => _behaviors;

        internal Plan(
            PlanIdentity identity,
            IReadOnlyDictionary<string, JsType> types,
            IReadOnlyDictionary<string, Component> components,
            IReadOnlyList<Behavior> behaviors)
        {
            _identity = identity;
            _types = types;
            _components = components;
            _behaviors = behaviors;
        }
    }
}
