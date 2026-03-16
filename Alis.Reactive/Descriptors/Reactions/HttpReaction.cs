using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Descriptors.Reactions
{
    public sealed class HttpReaction : Reaction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "http";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? PreFetch { get; }

        public RequestDescriptor Request { get; }

        public HttpReaction(List<Command>? preFetch, RequestDescriptor request)
        {
            PreFetch = preFetch;
            Request = request;
        }
    }
}
