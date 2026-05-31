using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered before the menu closes.
    /// </summary>
    public sealed class FusionMenuBeforeCloseArgs
    {
        public List<FusionMenuItem> Items { get; set; } = [];

        public FusionMenuItem? ParentItem { get; set; }

        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Typed mutations on the beforeClose event args for <see cref="FusionMenu"/>.
    /// </summary>
    public static class FusionMenuBeforeCloseArgsExtensions
    {
        public static void PreventClose(
            this FusionMenuBeforeCloseArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
