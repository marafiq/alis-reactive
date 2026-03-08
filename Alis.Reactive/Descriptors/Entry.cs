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

        public Entry(Trigger trigger, Reaction reaction)
        {
            Trigger = trigger;
            Reaction = reaction;
        }
    }
}
