using System;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Behavior
    {
        public StartsWhen StartsWhen { get; }
        public ReactionGraph Reaction { get; }

        private Behavior(StartsWhen startsWhen, ReactionGraph reaction)
        {
            StartsWhen = startsWhen ?? throw new ArgumentNullException(nameof(startsWhen));
            Reaction = reaction ?? throw new ArgumentNullException(nameof(reaction));
        }

        internal static Behavior On(StartsWhen trigger, ReactionGraph reaction) =>
            new Behavior(trigger, reaction);
    }
}
