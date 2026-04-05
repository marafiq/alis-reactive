using System;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Behavior
    {
        public StartsWhen StartsWhen { get; }
        public Reaction Reaction { get; }

        private Behavior(StartsWhen startsWhen, Reaction reaction)
        {
            StartsWhen = startsWhen ?? throw new ArgumentNullException(nameof(startsWhen));
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        }

        internal static Behavior On(StartsWhen trigger, Reaction reaction) =>
            new Behavior(trigger, reaction);
    }
}
