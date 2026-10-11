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
        private readonly IReadOnlyDictionary<string, VendorContract> _vendors;
        private readonly IReadOnlyDictionary<string, BrowserObject> _components;
        private readonly IReadOnlyList<Behavior> _behaviors;

        public int Version => 4;
        public string PlanId => _identity.PlanIdForJson;
        public PlanScope Scope => _identity.ScopeForJson;
        public IReadOnlyDictionary<string, VendorContract> Vendors => _vendors;
        public IReadOnlyDictionary<string, BrowserObject> Components => _components;
        public IReadOnlyList<Behavior> Behaviors => _behaviors;

        internal PlanDocument(
            PlanIdentity identity,
            IReadOnlyDictionary<string, VendorContract> vendors,
            IReadOnlyDictionary<string, BrowserObject> components,
            IReadOnlyList<Behavior> behaviors)
        {
            _identity = identity;
            _vendors = vendors;
            _components = components;
            _behaviors = behaviors;
        }
    }
}
