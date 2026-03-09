using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Descriptors.Reactions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(SequentialReaction), "sequential")]
    [JsonDerivedType(typeof(ConditionalReaction), "conditional")]
    [JsonDerivedType(typeof(HttpReaction), "http")]
    [JsonDerivedType(typeof(ParallelHttpReaction), "parallel-http")]
    public abstract class Reaction
    {
    }

    public sealed class SequentialReaction : Reaction
    {
        public List<Command> Commands { get; }

        public SequentialReaction(List<Command> commands)
        {
            Commands = commands;
        }
    }

    public sealed class ConditionalReaction : Reaction
    {
        public IReadOnlyList<Branch> Branches { get; }

        public ConditionalReaction(IReadOnlyList<Branch> branches)
        {
            Branches = branches;
        }
    }

    public sealed class HttpReaction : Reaction
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? PreFetch { get; }

        public RequestDescriptor Request { get; }

        public HttpReaction(List<Command>? preFetch, RequestDescriptor request)
        {
            PreFetch = preFetch;
            Request = request;
        }
    }

    public sealed class ParallelHttpReaction : Reaction
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? PreFetch { get; }

        public List<RequestDescriptor> Requests { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<StatusHandler>? OnAllSuccess { get; }

        public ParallelHttpReaction(
            List<Command>? preFetch,
            List<RequestDescriptor> requests,
            List<StatusHandler>? onAllSuccess = null)
        {
            PreFetch = preFetch;
            Requests = requests;
            OnAllSuccess = onAllSuccess;
        }
    }
}
