using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static partial class FusionGridExtensions
    {
        private static readonly ComponentMethod ShowColumnsByFieldMethod =
            ComponentMethod.Named("showColumns").WithArgs<string, string>();

        private static readonly ComponentMethod HideColumnsByFieldMethod =
            ComponentMethod.Named("hideColumns").WithArgs<string, string>();

        private static readonly ComponentMethod ReorderColumnsByFieldMethod =
            ComponentMethod.Named("reorderColumns").WithArgs<string, string>();

        /// <summary>
        /// Shows a runtime-hidden Grid column by typed row field.
        /// Initial column definitions remain owned by the Syncfusion MVC builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ShowColumn<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            Expression<Func<TRow, TField>> field)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(ShowColumnsByFieldMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(fieldName),
                ValueExpression.Literal("field")
            });
        }

        /// <summary>
        /// Hides a Grid column by typed row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> HideColumn<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            Expression<Func<TRow, TField>> field)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(HideColumnsByFieldMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(fieldName),
                ValueExpression.Literal("field")
            });
        }

        /// <summary>
        /// Moves one column before another using typed row fields.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ReorderColumnBefore<TModel, TRow, TFromField, TToField>(
            this ComponentRef<FusionGrid, TModel> self,
            Expression<Func<TRow, TFromField>> fromField,
            Expression<Func<TRow, TToField>> beforeField)
            where TModel : class
            where TRow : class
        {
            var fromFieldName = ExpressionPathHelper.ToEventPath(fromField);
            var beforeFieldName = ExpressionPathHelper.ToEventPath(beforeField);
            return self.EmitCall(ReorderColumnsByFieldMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(fromFieldName),
                ValueExpression.Literal(beforeFieldName)
            });
        }
    }
}
