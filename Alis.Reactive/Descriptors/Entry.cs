using System;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Descriptors
{
    /// <summary>
    /// One plan entry: a trigger paired with its reaction.
    /// The trigger says WHAT starts execution. The reaction says WHAT happens.
    /// </summary>
    public sealed class Entry
    {
        public Trigger Trigger { get; }
        public Reaction Reaction { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal Entry(Trigger trigger, Reaction reaction)
        {
            Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        }
    }
}
