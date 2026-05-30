using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered before the context menu opens.
    /// </summary>
    public sealed class FusionContextMenuBeforeOpenArgs
    {
        public List<FusionContextMenuItem> Items { get; set; } = [];

        public FusionContextMenuItem? ParentItem { get; set; }

        public double Top { get; set; }

        public double Left { get; set; }

        public bool IsFocused { get; set; }

        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Typed mutations on the beforeOpen event args for <see cref="FusionContextMenu"/>.
    /// </summary>
    public static class FusionContextMenuBeforeOpenArgsExtensions
    {
        public static void PreventOpen(
            this FusionContextMenuBeforeOpenArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
