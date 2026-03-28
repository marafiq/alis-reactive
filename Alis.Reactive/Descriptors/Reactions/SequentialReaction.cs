using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Descriptors.Reactions
{
    /// <summary>
    /// A reaction that executes its commands in order. Serialized as <c>kind: "sequential"</c>
    /// in the JSON plan. This is the default reaction type for simple trigger-to-command pipelines.
    /// </summary>
    public sealed class SequentialReaction : Reaction
    {
        /// <summary>Gets the type discriminator. Always <c>"sequential"</c>.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "sequential";

        /// <summary>Gets the ordered list of commands to execute when the trigger fires.</summary>
        public List<Command> Commands { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        /// <param name="commands">The ordered list of commands to execute sequentially.</param>
        internal SequentialReaction(List<Command> commands)
        {
            Commands = commands;
        }
    }
}
