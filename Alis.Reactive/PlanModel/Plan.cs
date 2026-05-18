using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Immutable plan document — the serialized contract between C# and the browser runtime.
    /// Produced by <see cref="PlanBuildContext.BuildPlan"/> once construction is complete.
    /// </summary>
    internal sealed class Plan
    {
        public int Version => 3;
        public string PlanId { get; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PartId { get; }
        public IReadOnlyDictionary<string, JsType> Types { get; }
        public IReadOnlyDictionary<string, Component> Components { get; }
        public IReadOnlyList<Behavior> Behaviors { get; }

        internal Plan(
            string planId,
            string? partId,
            IReadOnlyDictionary<string, JsType> types,
            IReadOnlyDictionary<string, Component> components,
            IReadOnlyList<Behavior> behaviors)
        {
            PlanId = planId ?? throw new System.ArgumentNullException(nameof(planId));
            PartId = partId;
            Types = types ?? throw new System.ArgumentNullException(nameof(types));
            Components = components ?? throw new System.ArgumentNullException(nameof(components));
            Behaviors = behaviors ?? throw new System.ArgumentNullException(nameof(behaviors));
        }
    }
}
