using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered before the menu opens.
    /// </summary>
    public sealed class FusionMenuBeforeOpenArgs
    {
        public List<FusionMenuItem> Items { get; set; } = [];

        public FusionMenuItem? ParentItem { get; set; }

        public double Top { get; set; }

        public double Left { get; set; }

        public bool IsFocused { get; set; }

        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Typed mutations on the beforeOpen event args for <see cref="FusionMenu"/>.
    /// </summary>
    public static class FusionMenuBeforeOpenArgsExtensions
    {
        public static void PreventOpen(
            this FusionMenuBeforeOpenArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
