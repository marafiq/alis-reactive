using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static partial class FusionGridExtensions
    {
        private static readonly ComponentMethod ExcelExportMethod =
            ComponentMethod.Named("excelExport");

        private static readonly ComponentMethod CsvExportMethod =
            ComponentMethod.Named("csvExport");

        private static readonly ComponentMethod PdfExportMethod =
            ComponentMethod.Named("pdfExport");

        private static readonly ComponentMethod PrintMethod =
            ComponentMethod.Named("print");

        private static readonly ComponentMethod OpenColumnChooserMethod =
            ComponentMethod.Named("openColumnChooser");

        // autoFitColumns is an EJ2 overload (0 args = all, 1 arg = one column). Distinct
        // plan member names mapped to the same JS path keep the plan contract merge
        // deterministic; a shared member name conflicts (0 vs 1 argument).
        private static readonly ComponentMethod AutoFitAllColumnsMethod =
            ComponentMethod.Mapped("autoFitColumnsAll", "autoFitColumns");

        private static readonly ComponentMethod AutoFitColumnMethod =
            ComponentMethod.Mapped("autoFitColumnsField", "autoFitColumns").WithArgs<string>();

        /// <summary>
        /// Exports the grid to Excel.
        /// Requires <c>AllowExcelExport(true)</c> on the builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ExcelExport<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(ExcelExportMethod);

        /// <summary>
        /// Exports the grid to CSV.
        /// Requires <c>AllowExcelExport(true)</c> on the builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> CsvExport<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(CsvExportMethod);

        /// <summary>
        /// Exports the grid to PDF.
        /// Requires <c>AllowPdfExport(true)</c> on the builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> PdfExport<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(PdfExportMethod);

        /// <summary>
        /// Opens the browser print dialog for the grid.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> Print<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(PrintMethod);

        /// <summary>
        /// Opens the column chooser.
        /// Requires <c>ShowColumnChooser(true)</c> on the builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ShowColumnChooser<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(OpenColumnChooserMethod);

        /// <summary>
        /// Auto-fits every grid column to its content.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> AutoFitColumns<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(AutoFitAllColumnsMethod);

        /// <summary>
        /// Auto-fits one grid column by typed row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> AutoFitColumn<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            Expression<Func<TRow, TField>> field)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(AutoFitColumnMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(fieldName)
            });
        }
    }
}
