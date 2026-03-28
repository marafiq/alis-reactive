using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Descriptors.Reactions
{
    public sealed class ParallelHttpReaction : Reaction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "parallel-http";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? PreFetch { get; }

        public List<RequestDescriptor> Requests { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? OnAllSettled { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ParallelHttpReaction(
            List<Command>? preFetch,
            List<RequestDescriptor> requests,
            List<Command>? onAllSettled = null)
        {
            PreFetch = preFetch;
            Requests = requests;
            OnAllSettled = onAllSettled;
        }
    }
}
