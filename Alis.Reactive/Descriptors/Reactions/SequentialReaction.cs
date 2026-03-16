using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Descriptors.Reactions
{
    public sealed class SequentialReaction : Reaction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "sequential";

        public List<Command> Commands { get; }

        public SequentialReaction(List<Command> commands)
        {
            Commands = commands;
        }
    }
}
