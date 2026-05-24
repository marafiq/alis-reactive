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
        private readonly PlanTypes _types;
        private readonly PlanComponents _components;
        private readonly PlanBehaviors _behaviors;

        public int Version => 3;
        public string PlanId => _identity.PlanIdForJson;
        public PlanScope Scope => _identity.ScopeForJson;
        public IReadOnlyDictionary<string, JsType> Types => _types.ForJson;
        public IReadOnlyDictionary<string, Component> Components => _components.ForJson;
        public IReadOnlyList<Behavior> Behaviors => _behaviors.ForJson;

        internal Plan(
            PlanIdentity identity,
            PlanTypes types,
            PlanComponents components,
            PlanBehaviors behaviors)
        {
            _identity = identity ?? throw new System.ArgumentNullException(nameof(identity));
            _types = types ?? throw new System.ArgumentNullException(nameof(types));
            _components = components ?? throw new System.ArgumentNullException(nameof(components));
            _behaviors = behaviors ?? throw new System.ArgumentNullException(nameof(behaviors));
        }
    }

    internal sealed class PlanTypes
    {
        private readonly IReadOnlyDictionary<string, JsType> _types;

        private PlanTypes(IReadOnlyDictionary<string, JsType> types)
        {
            _types = types;
        }

        internal IReadOnlyDictionary<string, JsType> ForJson => _types;

        internal static PlanTypes From(IReadOnlyDictionary<string, JsType> types)
        {
            if (types == null) throw new System.ArgumentNullException(nameof(types));

            var snapshot = new Dictionary<string, JsType>(System.StringComparer.Ordinal);
            foreach (var type in types)
            {
                TypeKey.Of(type.Key);
                snapshot[type.Key] = type.Value ?? throw new System.ArgumentException(
                    "Plan type entry must not be null.",
                    nameof(types));
            }

            return new PlanTypes(snapshot);
        }
    }

    internal sealed class PlanComponents
    {
        private readonly IReadOnlyDictionary<string, Component> _components;

        private PlanComponents(IReadOnlyDictionary<string, Component> components)
        {
            _components = components;
        }

        internal IReadOnlyDictionary<string, Component> ForJson => _components;

        internal static PlanComponents From(IReadOnlyDictionary<string, Component> components)
        {
            if (components == null) throw new System.ArgumentNullException(nameof(components));

            var snapshot = new Dictionary<string, Component>(System.StringComparer.Ordinal);
            foreach (var component in components)
            {
                ComponentKey.Of(component.Key);
                snapshot[component.Key] = component.Value ?? throw new System.ArgumentException(
                    "Plan component entry must not be null.",
                    nameof(components));
            }

            return new PlanComponents(snapshot);
        }
    }

    internal sealed class PlanBehaviors
    {
        private readonly IReadOnlyList<Behavior> _behaviors;

        private PlanBehaviors(IReadOnlyList<Behavior> behaviors)
        {
            _behaviors = behaviors;
        }

        internal IReadOnlyList<Behavior> ForJson => _behaviors;

        internal static PlanBehaviors From(IReadOnlyList<Behavior> behaviors)
        {
            if (behaviors == null) throw new System.ArgumentNullException(nameof(behaviors));

            var snapshot = new List<Behavior>();
            foreach (var behavior in behaviors)
            {
                if (behavior == null)
                    throw new System.ArgumentException("Plan behavior must not be null.", nameof(behaviors));

                snapshot.Add(behavior);
            }

            return new PlanBehaviors(snapshot);
        }
    }
}
