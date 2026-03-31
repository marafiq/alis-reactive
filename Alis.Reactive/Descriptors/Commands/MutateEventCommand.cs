using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Descriptors.Commands
{
    /// <summary>
    /// Mutates the event args object (ctx.evt) that triggered this reaction.
    /// Supports both set-prop (e.g., e.preventDefaultAction = true) and
    /// call (e.g., e.updateData(data)).
    ///
    /// Same mutation algebra as MutateElementCommand, but the target is
    /// the event args object, not a DOM element. No target ID or vendor needed —
    /// the runtime resolves ctx.evt directly.
    /// </summary>
    public sealed class MutateEventCommand : Command
    {
        /// <summary>Gets the discriminator written to plan JSON.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "mutate-event";

        /// <summary>Gets the mutation applied to the triggering event object.</summary>
        public Mutation Mutation { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal MutateEventCommand(Mutation mutation)
        {
            Mutation = mutation;
        }
    }
}
