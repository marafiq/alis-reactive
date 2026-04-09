using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Plan
    {
        public int Version => 3;
        public string PlanId { get; }
        public string? PartId { get; internal set; }

        // Mutable backing fields — accessed by PlanBuildContext during construction
        internal Dictionary<string, JsType> MutableTypes { get; }
        internal Dictionary<string, Component> MutableComponents { get; }
        internal List<Behavior> MutableBehaviors { get; }

        // Read-only public surface for serialization and external consumption
        public IReadOnlyDictionary<string, JsType> Types => MutableTypes;
        public IReadOnlyDictionary<string, Component> Components => MutableComponents;
        public IReadOnlyList<Behavior> Behaviors => MutableBehaviors;

        private Plan(string planId)
        {
            PlanId = planId ?? throw new System.ArgumentNullException(nameof(planId));
            MutableTypes = new Dictionary<string, JsType>();
            MutableComponents = new Dictionary<string, Component>();
            MutableBehaviors = new List<Behavior>();
        }

        internal static Plan Create(string planId, string? partId = null)
        {
            return new Plan(planId) { PartId = partId };
        }

        internal Plan WithType(string key, JsType type)
        {
            MutableTypes[key] = type;
            return this;
        }

        internal Plan WithComponent(string key, Component component)
        {
            MutableComponents[key] = component;
            return this;
        }

        internal Plan WithBehavior(Behavior behavior)
        {
            MutableBehaviors.Add(behavior);
            return this;
        }
    }
}
