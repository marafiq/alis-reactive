using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Immutable plan document: the serialized contract between C# authoring and the Reactive Plan runtime.
    /// Produced by <see cref="PlanBuildContext.BuildPlan"/> once construction is complete.
    /// </summary>
    internal sealed class PlanDocument
    {
        private readonly PlanIdentity _identity;
        private readonly IReadOnlyDictionary<string, BrowserObjectContract> _types;
        private readonly IReadOnlyDictionary<string, BrowserObject> _components;
        private readonly IReadOnlyList<Behavior> _behaviors;

        public int Version => 3;
        public string PlanId => _identity.PlanIdForJson;
        public PlanScope Scope => _identity.ScopeForJson;
        public IReadOnlyDictionary<string, BrowserObjectContract> Types => _types;
        public IReadOnlyDictionary<string, BrowserObject> Components => _components;
        public IReadOnlyList<Behavior> Behaviors => _behaviors;

        internal PlanDocument(
            PlanIdentity identity,
            IReadOnlyDictionary<string, BrowserObjectContract> types,
            IReadOnlyDictionary<string, BrowserObject> components,
            IReadOnlyList<Behavior> behaviors)
        {
            _identity = identity;
            _types = types;
            _components = components;
            _behaviors = behaviors;
        }
    }
}
