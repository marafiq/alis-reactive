using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Descriptors.Reactions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(SequentialReaction), "sequential")]
    [JsonDerivedType(typeof(ConditionalReaction), "conditional")]
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
}
