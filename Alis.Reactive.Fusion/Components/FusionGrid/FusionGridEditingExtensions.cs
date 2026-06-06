using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static partial class FusionGridExtensions
    {
        private static readonly ComponentMethod StartEditMethod =
            ComponentMethod.Named("startEdit");

        private static readonly ComponentMethod EndEditMethod =
            ComponentMethod.Named("endEdit");

        private static readonly ComponentMethod CloseEditMethod =
            ComponentMethod.Named("closeEdit");

        private static readonly ComponentMethod AddRecordMethod =
            ComponentMethod.Named("addRecord").WithArgs<object>();

        private static readonly ComponentMethod AddRecordAtMethod =
            ComponentMethod.Named("addRecord").WithArgs<object, int>();

        private static readonly ComponentMethod DeleteRecordMethod =
            ComponentMethod.Named("deleteRecord");

        private static readonly ComponentMethod UpdateRowMethod =
            ComponentMethod.Named("updateRow").WithArgs<int, object>();

        private static readonly ComponentMethod EditCellMethod =
            ComponentMethod.Named("editCell").WithArgs<int, string>();

        private static readonly ComponentMethod SaveCellMethod =
            ComponentMethod.Named("saveCell");

        private static readonly ComponentMethod UpdateStringCellMethod =
            ComponentMethod.Named("updateCell").WithArgs<int, string, string>();

        private static readonly ComponentMethod UpdateIntCellMethod =
            ComponentMethod.Named("updateCell").WithArgs<int, string, int>();

        private static readonly ComponentMethod GetBatchChangesMethod =
            ComponentMethod.Named("getBatchChanges");

        /// <summary>
        /// Starts editing the selected row.
        /// The builder-owned editSettings determine whether this is inline or dialog editing.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> StartEdit<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(StartEditMethod);

        /// <summary>
        /// Saves the active edit. In batch mode this commits pending batch changes.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> EndEdit<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(EndEditMethod);

        /// <summary>
        /// Cancels the active edit state.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> CloseEdit<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(CloseEditMethod);

        /// <summary>
        /// Adds a typed row.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> AddRecord<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            TRow row,
            int? index = null)
            where TModel : class
            where TRow : class
        {
            var rowValue = ValueExpression.LiteralRaw(row, Shape.FromClrType(typeof(TRow)));
            return index.HasValue
                ? self.EmitCall(AddRecordAtMethod, new List<ValueExpression> { rowValue, ValueExpression.Literal(index.Value) })
                : self.EmitCall(AddRecordMethod, new List<ValueExpression> { rowValue });
        }

        /// <summary>
        /// Adds a typed row from an HTTP response.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> AddRecord<TModel, TResponse, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, TRow>> path,
            int? index = null)
            where TModel : class
            where TResponse : class
            where TRow : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            var rowValue = ValueExpression.Read(source.Scope, sourcePath, Shape.FromClrType(typeof(TRow)));
            return index.HasValue
                ? self.EmitCall(AddRecordAtMethod, new List<ValueExpression> { rowValue, ValueExpression.Literal(index.Value) })
                : self.EmitCall(AddRecordMethod, new List<ValueExpression> { rowValue });
        }

        /// <summary>
        /// Deletes the selected Grid record.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> DeleteSelectedRecord<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(DeleteRecordMethod);

        /// <summary>
        /// Updates one rendered row with a typed row.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> UpdateRow<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            int rowIndex,
            TRow row)
            where TModel : class
            where TRow : class
        {
            var rowValue = ValueExpression.LiteralRaw(row, Shape.FromClrType(typeof(TRow)));
            return self.EmitCall(UpdateRowMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(rowIndex),
                rowValue
            });
        }

        /// <summary>
        /// Updates one rendered row from an HTTP response.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> UpdateRow<TModel, TResponse, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            int rowIndex,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, TRow>> path)
            where TModel : class
            where TResponse : class
            where TRow : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            var rowValue = ValueExpression.Read(source.Scope, sourcePath, Shape.FromClrType(typeof(TRow)));
            return self.EmitCall(UpdateRowMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(rowIndex),
                rowValue
            });
        }

        /// <summary>
        /// Enters batch cell edit mode for a typed row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> EditCell<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            int rowIndex,
            Expression<Func<TRow, TField>> field)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(EditCellMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(rowIndex),
                ValueExpression.Literal(fieldName)
            });
        }

        /// <summary>
        /// Saves the edited batch cell.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SaveCell<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(SaveCellMethod);

        public static ComponentRef<FusionGrid, TModel> UpdateCell<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            int rowIndex,
            Expression<Func<TRow, string>> field,
            string value)
            where TModel : class
            where TRow : class
            => self.UpdateCell(rowIndex, field, ValueExpression.Literal(value), UpdateStringCellMethod);

        public static ComponentRef<FusionGrid, TModel> UpdateCell<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            int rowIndex,
            Expression<Func<TRow, int>> field,
            int value)
            where TModel : class
            where TRow : class
            => self.UpdateCell(rowIndex, field, ValueExpression.Literal(value), UpdateIntCellMethod);

        /// <summary>
        /// Reads typed batch edit changes.
        /// </summary>
        public static TypedComponentSource<FusionGridBatchChanges<TRow>> BatchChanges<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            where TRow : class
            => self.Read<FusionGridBatchChanges<TRow>>(GetBatchChangesMethod);

        private static ComponentRef<FusionGrid, TModel> UpdateCell<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            int rowIndex,
            Expression<Func<TRow, TField>> field,
            ValueExpression value,
            ComponentMethod method)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(method, new List<ValueExpression>
            {
                ValueExpression.Literal(rowIndex),
                ValueExpression.Literal(fieldName),
                value
            });
        }
    }
}
