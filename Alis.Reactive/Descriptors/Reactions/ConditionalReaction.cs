using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Descriptors.Reactions
{
    public sealed class ConditionalReaction : Reaction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "conditional";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? Commands { get; }

        public IReadOnlyList<Branch> Branches { get; }

        public ConditionalReaction(List<Command>? commands, IReadOnlyList<Branch> branches)
        {
            Commands = commands;
            Branches = branches;
        }
    }
}
