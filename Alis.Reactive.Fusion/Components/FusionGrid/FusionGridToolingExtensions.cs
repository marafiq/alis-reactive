using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
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

        private static readonly ComponentMethod AutoFitAllColumnsMethod =
            ComponentMethod.Named("autoFitColumns");

        private static readonly ComponentMethod AutoFitColumnMethod =
            ComponentMethod.Named("autoFitColumns").WithArgs<string>();

        /// <summary>
        /// Exports the grid to Excel through Syncfusion's public excelExport method.
        /// Requires <c>AllowExcelExport(true)</c> on the builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ExcelExport<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(ExcelExportMethod);

        /// <summary>
        /// Exports the grid to CSV through Syncfusion's public csvExport method.
        /// Requires <c>AllowExcelExport(true)</c> on the builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> CsvExport<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(CsvExportMethod);

        /// <summary>
        /// Exports the grid to PDF through Syncfusion's public pdfExport method.
        /// Requires <c>AllowPdfExport(true)</c> on the builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> PdfExport<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(PdfExportMethod);

        /// <summary>
        /// Opens the browser print dialog for the grid through Syncfusion's public print method.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> Print<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(PrintMethod);

        /// <summary>
        /// Opens the column chooser through Syncfusion's public openColumnChooser method.
        /// Requires <c>ShowColumnChooser(true)</c> on the builder.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> ShowColumnChooser<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(OpenColumnChooserMethod);

        /// <summary>
        /// Auto-fits every grid column to its content through Syncfusion's public autoFitColumns method.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> AutoFitColumns<TModel>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            => self.EmitCall(AutoFitAllColumnsMethod);

        /// <summary>
        /// Auto-fits one grid column by typed row field through Syncfusion's public autoFitColumns method.
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
