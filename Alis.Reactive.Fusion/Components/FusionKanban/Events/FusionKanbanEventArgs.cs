using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public sealed class FusionKanbanEmptyArgs
    {
    }

    public sealed class FusionKanbanActionArgs<TCard>
        where TCard : class, new()
    {
        public string RequestType { get; set; } = "";
        public bool Cancel { get; set; }
        public List<TCard> AddedRecords { get; set; } = new List<TCard>();
        public List<TCard> ChangedRecords { get; set; } = new List<TCard>();
        public List<TCard> DeletedRecords { get; set; } = new List<TCard>();
    }

    public sealed class FusionKanbanDataBindingArgs<TCard>
        where TCard : class, new()
    {
        public List<TCard> Result { get; set; } = new List<TCard>();
        public int? Count { get; set; }
    }

    public sealed class FusionKanbanCardClickArgs<TCard>
        where TCard : class, new()
    {
        public TCard Data { get; set; } = new TCard();
        public bool Cancel { get; set; }
    }

    public sealed class FusionKanbanCardRenderedArgs<TCard>
        where TCard : class, new()
    {
        public TCard Data { get; set; } = new TCard();
        public bool Cancel { get; set; }
    }

    public sealed class FusionKanbanDialogArgs<TCard>
        where TCard : class, new()
    {
        public TCard Data { get; set; } = new TCard();
        public bool Cancel { get; set; }
        public string RequestType { get; set; } = "";
    }

    public sealed class FusionKanbanDragArgs<TCard>
        where TCard : class, new()
    {
        public List<TCard> Data { get; set; } = new List<TCard>();
        public bool Cancel { get; set; }
        public int DropIndex { get; set; }
    }

    public sealed class FusionKanbanHeaderArgs
    {
        public string KeyField { get; set; } = "";
        public string TextField { get; set; } = "";
        public int Count { get; set; }
    }

    public sealed class FusionKanbanQueryCellInfoArgs
    {
        public List<FusionKanbanHeaderArgs> Data { get; set; } = new List<FusionKanbanHeaderArgs>();
        public bool Cancel { get; set; }
        public string RequestType { get; set; } = "";
    }

    public sealed class FusionKanbanDataSourceChangedArgs<TCard>
        where TCard : class, new()
    {
        public string RequestType { get; set; } = "";
        public List<TCard> AddedRecords { get; set; } = new List<TCard>();
        public List<TCard> ChangedRecords { get; set; } = new List<TCard>();
        public List<TCard> DeletedRecords { get; set; } = new List<TCard>();
        public int Index { get; set; }
    }

    public static class FusionKanbanEventArgsExtensions
    {
        public static void Cancel<TCard>(
            this FusionKanbanActionArgs<TCard> args,
            IReactionEmitter pipeline)
            where TCard : class, new()
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }

        public static void Cancel<TCard>(
            this FusionKanbanCardClickArgs<TCard> args,
            IReactionEmitter pipeline)
            where TCard : class, new()
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }

        public static void Cancel<TCard>(
            this FusionKanbanDialogArgs<TCard> args,
            IReactionEmitter pipeline)
            where TCard : class, new()
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }

        public static void Cancel(
            this FusionKanbanQueryCellInfoArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }

        public static void Cancel<TCard>(
            this FusionKanbanDragArgs<TCard> args,
            IReactionEmitter pipeline)
            where TCard : class, new()
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }

        public static void EndEdit<TCard>(
            this FusionKanbanDataSourceChangedArgs<TCard> args,
            IReactionEmitter pipeline)
            where TCard : class, new()
        {
            pipeline.AddStep(ReactionGraph.Call(
                PayloadSource.Event(),
                "endEdit",
                System.Array.Empty<ValueExpression>()));
        }

        public static void CancelEdit<TCard>(
            this FusionKanbanDataSourceChangedArgs<TCard> args,
            IReactionEmitter pipeline)
            where TCard : class, new()
        {
            pipeline.AddStep(ReactionGraph.Call(
                PayloadSource.Event(),
                "cancelEdit",
                System.Array.Empty<ValueExpression>()));
        }
    }
}
