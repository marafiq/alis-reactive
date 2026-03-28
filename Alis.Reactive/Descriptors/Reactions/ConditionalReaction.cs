using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Descriptors.Reactions
{
    /// <summary>
    /// A reaction that evaluates guarded branches in order, executing the first branch
    /// whose guard passes. Serialized as <c>kind: "conditional"</c> in the JSON plan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Commands"/> contains unconditional commands that run before branch
    /// evaluation (e.g., HTTP requests whose response feeds into conditions). When no
    /// unconditional commands exist, this property serializes as <see langword="null"/>
    /// and is omitted from the JSON output.
    /// </para>
    /// <para>
    /// <see cref="Branches"/> is evaluated top-to-bottom. The first branch with a passing
    /// guard (or a <see langword="null"/> guard for the <c>Else</c> branch) wins. This
    /// mirrors <c>if/else-if/else</c> semantics.
    /// </para>
    /// </remarks>
    public sealed class ConditionalReaction : Reaction
    {
        /// <summary>Gets the type discriminator. Always <c>"conditional"</c>.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "conditional";

        /// <summary>
        /// Gets the unconditional commands that execute before branch evaluation, or
        /// <see langword="null"/> when the reaction has no pre-condition commands.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? Commands { get; }

        /// <summary>Gets the ordered list of guard-reaction pairs evaluated top-to-bottom.</summary>
        public IReadOnlyList<Branch> Branches { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        public ConditionalReaction(List<Command>? commands, IReadOnlyList<Branch> branches)
        {
            Commands = commands;
            Branches = branches;
        }
    }
}
