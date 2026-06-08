using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public class FusionGridBatchChanges<TRow>
        where TRow : class
    {
        public List<TRow> AddedRecords { get; set; } = new List<TRow>();
        public List<TRow> ChangedRecords { get; set; } = new List<TRow>();
        public List<TRow> DeletedRecords { get; set; } = new List<TRow>();
    }

    public class FusionGridBeginEditArgs<TRow>
        where TRow : class
    {
        public TRow RowData { get; set; } = default!;
        public int RowIndex { get; set; }
        public string? Type { get; set; }
        public bool Cancel { get; set; }
    }

    public class FusionGridEditActionArgs<TRow>
        where TRow : class
    {
        public string? Name { get; set; }
        public string? RequestType { get; set; }
        public string? Action { get; set; }
        public string? Type { get; set; }
        public bool Cancel { get; set; }
        public TRow Data { get; set; } = default!;
        public TRow PreviousData { get; set; } = default!;
        public int? RowIndex { get; set; }
        public int SelectedRow { get; set; }
    }

    public class FusionGridCellSaveArgs<TRow, TValue>
        where TRow : class
    {
        public TRow RowData { get; set; } = default!;
        public string? ColumnName { get; set; }
        public TValue? Value { get; set; }
        public TValue? PreviousValue { get; set; }
        public bool Cancel { get; set; }
    }

    public class FusionGridCellSavedArgs<TRow, TValue>
        where TRow : class
    {
        public TRow RowData { get; set; } = default!;
        public string? ColumnName { get; set; }
        public TValue? Value { get; set; }
        public TValue? PreviousValue { get; set; }
    }

    public class FusionGridBeforeBatchSaveArgs<TRow>
        where TRow : class
    {
        public FusionGridBatchChanges<TRow> BatchChanges { get; set; } = new FusionGridBatchChanges<TRow>();
        public bool Cancel { get; set; }
    }

    public static class FusionGridEditEventArgsExtensions
    {
        public static void Cancel<TRow>(
            this FusionGridBeginEditArgs<TRow> args,
            IReactionEmitter pipeline)
            where TRow : class
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }

        public static void Cancel<TRow>(
            this FusionGridEditActionArgs<TRow> args,
            IReactionEmitter pipeline)
            where TRow : class
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }

        public static void Cancel<TRow, TValue>(
            this FusionGridCellSaveArgs<TRow, TValue> args,
            IReactionEmitter pipeline)
            where TRow : class
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }

        public static void Cancel<TRow>(
            this FusionGridBeforeBatchSaveArgs<TRow> args,
            IReactionEmitter pipeline)
            where TRow : class
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }
    }
}
