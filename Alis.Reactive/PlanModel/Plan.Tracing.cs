using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Tracing fields for the reactive plan. Kept in a partial file so the
    /// core plan model stays focused on structure and the tracing surface
    /// can evolve independently. Values flow from the server render path
    /// (ambient <c>System.Diagnostics.Activity</c>) into the plan JSON and
    /// into the browser runtime's <c>configure()</c> call at boot.
    /// </summary>
    internal sealed partial class Plan
    {
        /// <summary>
        /// W3C traceparent header value describing the server side distributed
        /// trace that produced this plan. When set, the browser runtime uses it
        /// to seed its initial interaction root so every page ready behavior,
        /// and every outbound HTTP request triggered by those behaviors,
        /// correlates back to the server response trace. Populated at render
        /// time from the ambient <c>System.Diagnostics.Activity</c>, so every
        /// server rendered page carries its ASP.NET Core activity trace with
        /// zero developer involvement.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("traceparent")]
        internal string? Traceparent { get; set; }

        /// <summary>
        /// Optional tracing level that the browser runtime applies via
        /// <c>configure({ level })</c> at boot time. Valid values match the
        /// TS runtime's <c>Level</c> type: <c>off</c>, <c>error</c>, <c>warn</c>,
        /// <c>info</c>, <c>debug</c>, <c>trace</c>. Null means the runtime uses
        /// whatever the <c>data-trace</c> attribute on the plan element says,
        /// or <c>off</c> if neither is present.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("traceLevel")]
        internal string? TraceLevel { get; set; }
    }
}
