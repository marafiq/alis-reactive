using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Descriptors.Reactions
{
    /// <summary>
    /// A guard-reaction pair inside a <see cref="ConditionalReaction"/>. If the
    /// <see cref="Guard"/> evaluates to <see langword="true"/> (or is <see langword="null"/>
    /// for an <c>Else</c> branch), the <see cref="Reaction"/> executes.
    /// </summary>
    /// <remarks>
    /// Branches are evaluated in declaration order by the runtime. A <see langword="null"/>
    /// guard represents the unconditional fallback (<c>Else</c>) and must be the last branch.
    /// The guard is omitted from the JSON output when <see langword="null"/>.
    /// </remarks>
    public sealed class Branch
    {
        /// <summary>
        /// Gets the guard condition for this branch, or <see langword="null"/> for
        /// an unconditional <c>Else</c> fallback.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guard? Guard { get; }

        /// <summary>Gets the reaction that executes when the guard passes.</summary>
        public Reaction Reaction { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal Branch(Guard? guard, Reaction reaction)
        {
            Guard = guard;
            Reaction = reaction;
        }
    }
}
