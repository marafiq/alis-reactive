using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static partial class FusionGridExtensions
    {
        private static readonly ComponentMethod GoToPageMethod =
            ComponentMethod.Named("goToPage").WithArgs<int>();

        private static readonly ComponentMethod SortColumnMethod =
            ComponentMethod.Named("sortColumn").WithArgs<string, string, bool>();

        private static readonly ComponentMethod ClearSortingMethod =
            ComponentMethod.Named("clearSorting");

        private static readonly ComponentMethod SearchMethod =
            ComponentMethod.Named("search").WithArgs<string>();

        private static readonly ComponentMethod FilterByColumnTextMethod =
            ComponentMethod.Named("filterByColumn").WithArgs<string, string, string>();

        private static readonly ComponentMethod ClearFilteringMethod =
            ComponentMethod.Named("clearFiltering");

        private static readonly ComponentMethod GroupColumnMethod =
            ComponentMethod.Named("groupColumn").WithArgs<string>();

        private static readonly ComponentMethod UngroupColumnMethod =
            ComponentMethod.Named("ungroupColumn").WithArgs<string>();

        private static readonly ComponentMethod ClearGroupingMethod =
            ComponentMethod.Named("clearGrouping");

        /// <summary>
        /// Navigates the grid to a page.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> GoToPage<TModel>(
            this ComponentRef<FusionGrid, TModel> self,
            int pageNumber)
            where TModel : class
            => self.EmitCall(GoToPageMethod, new List<ValueExpression> { ValueExpression.Literal(pageNumber) });

        /// <summary>
        /// Sorts a typed row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SortBy<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            Expression<Func<TRow, TField>> field,
            FusionGridSortDirection direction,
            bool keepExistingSorts = false)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(SortColumnMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(fieldName),
                ValueExpression.Literal(ToSyncfusion(direction)),
                ValueExpression.Literal(keepExistingSorts)
            });
        }

        /// <summary>
        /// Clears all grid sorting.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ClearSorting<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(ClearSortingMethod);

        /// <summary>
        /// Searches grid records.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> Search<TModel>(
            this ComponentRef<FusionGrid, TModel> self,
            string searchText)
            where TModel : class
            => self.EmitCall(SearchMethod, new List<ValueExpression> { ValueExpression.Literal(searchText) });

        /// <summary>
        /// Clears the current grid search.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ClearSearch<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.Search(string.Empty);

        /// <summary>
        /// Applies a text filter to a typed row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> FilterTextBy<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            Expression<Func<TRow, TField>> field,
            FusionGridTextFilterOperator filterOperator,
            string value)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(FilterByColumnTextMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(fieldName),
                ValueExpression.Literal(ToSyncfusion(filterOperator)),
                ValueExpression.Literal(value)
            });
        }

        /// <summary>
        /// Clears all grid filtering.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ClearFiltering<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(ClearFilteringMethod);

        /// <summary>
        /// Groups a typed row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> GroupBy<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            Expression<Func<TRow, TField>> field)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(GroupColumnMethod, new List<ValueExpression> { ValueExpression.Literal(fieldName) });
        }

        /// <summary>
        /// Ungroups a typed row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> UngroupBy<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            Expression<Func<TRow, TField>> field)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(UngroupColumnMethod, new List<ValueExpression> { ValueExpression.Literal(fieldName) });
        }

        /// <summary>
        /// Clears all grouping.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ClearGrouping<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(ClearGroupingMethod);

        private static string ToSyncfusion(FusionGridSortDirection direction) =>
            direction == FusionGridSortDirection.Descending ? "Descending" : "Ascending";

        private static string ToSyncfusion(FusionGridTextFilterOperator filterOperator) =>
            filterOperator switch
            {
                FusionGridTextFilterOperator.Equal => "equal",
                FusionGridTextFilterOperator.NotEqual => "notequal",
                FusionGridTextFilterOperator.StartsWith => "startswith",
                FusionGridTextFilterOperator.EndsWith => "endswith",
                _ => "contains"
            };
    }
}
