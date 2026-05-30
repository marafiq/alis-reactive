using System.Collections.Generic;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static partial class FusionGridExtensions
    {
        private static readonly ComponentMethod SelectRowMethod =
            ComponentMethod.Named("selectRow").WithArgs<int>();

        private static readonly ComponentMethod ClearSelectionMethod =
            ComponentMethod.Named("clearSelection");

        private static readonly ComponentMethod GetSelectedRowIndexesMethod =
            ComponentMethod.Named("getSelectedRowIndexes");

        private static readonly ComponentMethod GetSelectedRecordsMethod =
            ComponentMethod.Named("getSelectedRecords");

        /// <summary>
        /// Selects one rendered row by zero-based row index.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SelectRow<TModel>(
            this ComponentRef<FusionGrid, TModel> self,
            int rowIndex)
            where TModel : class
            => self.EmitCall(SelectRowMethod, new List<ValueExpression> { ValueExpression.Literal(rowIndex) });

        /// <summary>
        /// Clears grid selection through Syncfusion's public clearSelection method.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ClearSelection<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(ClearSelectionMethod);

        /// <summary>
        /// Reads selected row indexes through Syncfusion's public getSelectedRowIndexes method.
        /// </summary>
        public static TypedComponentSource<int[]> SelectedRowIndexes<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.Read<int[]>(GetSelectedRowIndexesMethod);

        /// <summary>
        /// Reads selected row records through Syncfusion's public getSelectedRecords method.
        /// </summary>
        public static TypedComponentSource<TRow[]> SelectedRecords<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            where TRow : class
            => self.Read<TRow[]>(GetSelectedRecordsMethod);
    }
}
