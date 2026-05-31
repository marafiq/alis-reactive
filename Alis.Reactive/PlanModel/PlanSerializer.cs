using System.Text.Json;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The single owner of plan-document to JSON. Emits camelCase property names; node <c>kind</c>
    /// values pass through verbatim. Compact for transport, formatted for debugging.
    /// </summary>
    internal static class PlanSerializer
    {
        private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions Formatted = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>Serializes the plan document to compact camelCase JSON for the data-reactive-plan script.</summary>
        internal static string Serialize(PlanDocument plan) => JsonSerializer.Serialize(plan, Compact);

        /// <summary>Serializes the plan document to indented camelCase JSON for debugging.</summary>
        internal static string SerializeFormatted(PlanDocument plan) => JsonSerializer.Serialize(plan, Formatted);
    }
}
