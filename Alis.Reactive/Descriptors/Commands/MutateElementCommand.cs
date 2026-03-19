using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Descriptors.Commands
{
    public sealed class MutateElementCommand : Command
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "mutate-element";

        public string Target { get; }
        public Mutation Mutation { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Value { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BindSource? Source { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Vendor { get; }

        public MutateElementCommand(
            string target,
            Mutation mutation,
            object? value = null,
            BindSource? source = null,
            string? vendor = null)
        {
            Target = target;
            Mutation = mutation;
            Value = value;
            Source = source;
            Vendor = vendor;
        }
    }
}
