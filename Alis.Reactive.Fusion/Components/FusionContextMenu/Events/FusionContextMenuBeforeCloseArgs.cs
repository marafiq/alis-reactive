using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered before the context menu closes.
    /// </summary>
    public sealed class FusionContextMenuBeforeCloseArgs
    {
        public List<FusionContextMenuItem> Items { get; set; } = [];

        public FusionContextMenuItem? ParentItem { get; set; }

        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Typed mutations on the beforeClose event args for <see cref="FusionContextMenu"/>.
    /// </summary>
    public static class FusionContextMenuBeforeCloseArgsExtensions
    {
        public static void PreventClose(
            this FusionContextMenuBeforeCloseArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
