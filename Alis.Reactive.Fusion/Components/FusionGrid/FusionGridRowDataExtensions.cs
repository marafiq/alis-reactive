using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    public static partial class FusionGridExtensions
    {
        private static readonly ComponentMethod GetCurrentViewRecordsMethod =
            ComponentMethod.Named("getCurrentViewRecords");

        private static readonly ComponentMethod GetRowIndexByIntPrimaryKeyMethod =
            ComponentMethod.Named("getRowIndexByPrimaryKey").WithArgs<int>();

        private static readonly ComponentMethod SetIntCellValueMethod =
            ComponentMethod.Mapped("setIntCellValue", "setCellValue").WithArgs<int, string, int>();

        private static readonly ComponentMethod SetStringCellValueMethod =
            ComponentMethod.Mapped("setStringCellValue", "setCellValue").WithArgs<int, string, string>();

        private static readonly ComponentMethod SetRowDataByIntKeyMethod =
            ComponentMethod.Named("setRowData").WithArgs<int, object>();

        /// <summary>
        /// Reads the Grid's current rendered view records through Syncfusion's public getCurrentViewRecords method.
        /// </summary>
        public static TypedComponentSource<TRow[]> CurrentViewRecords<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self)
            where TModel : class
            where TRow : class
            => self.Read<TRow[]>(GetCurrentViewRecordsMethod);

        /// <summary>
        /// Reads the visible row index for a numeric primary key through Syncfusion's public getRowIndexByPrimaryKey method.
        /// </summary>
        public static TypedComponentSource<int> RowIndexByPrimaryKey<TModel>(
            this ComponentRef<FusionGrid, TModel> self,
            int primaryKey)
            where TModel : class
            => self.Read<int>(
                GetRowIndexByIntPrimaryKeyMethod,
                new List<ValueExpression> { ValueExpression.Literal(primaryKey) });

        /// <summary>
        /// Updates one visible cell by numeric primary key and typed string row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetCellValue<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            int primaryKey,
            Expression<Func<TRow, string>> field,
            string value)
            where TModel : class
            where TRow : class
            => self.SetCellValue(primaryKey, field, ValueExpression.Literal(value), SetStringCellValueMethod);

        /// <summary>
        /// Updates one visible cell by numeric primary key and typed integer row field.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetCellValue<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            int primaryKey,
            Expression<Func<TRow, int>> field,
            int value)
            where TModel : class
            where TRow : class
            => self.SetCellValue(primaryKey, field, ValueExpression.Literal(value), SetIntCellValueMethod);

        /// <summary>
        /// Updates one visible row by numeric primary key with a typed row payload.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetRowData<TModel, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            int primaryKey,
            TRow row)
            where TModel : class
            where TRow : class
        {
            var rowValue = ValueExpression.LiteralRaw(row, Shape.FromClrType(typeof(TRow)));
            return self.EmitCall(SetRowDataByIntKeyMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(primaryKey),
                rowValue
            });
        }

        /// <summary>
        /// Updates one visible row by numeric primary key with a typed row from an HTTP response.
        /// </summary>
        public static ComponentRef<FusionGrid, TModel> SetRowData<TModel, TResponse, TRow>(
            this ComponentRef<FusionGrid, TModel> self,
            int primaryKey,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, TRow>> path)
            where TModel : class
            where TResponse : class
            where TRow : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            var rowValue = ValueExpression.Read(source.Scope, sourcePath, Shape.FromClrType(typeof(TRow)));
            return self.EmitCall(SetRowDataByIntKeyMethod, new List<ValueExpression>
            {
                ValueExpression.Literal(primaryKey),
                rowValue
            });
        }

        private static ComponentRef<FusionGrid, TModel> SetCellValue<TModel, TRow, TField>(
            this ComponentRef<FusionGrid, TModel> self,
            int primaryKey,
            Expression<Func<TRow, TField>> field,
            ValueExpression value,
            ComponentMethod method)
            where TModel : class
            where TRow : class
        {
            var fieldName = ExpressionPathHelper.ToEventPath(field);
            return self.EmitCall(method, new List<ValueExpression>
            {
                ValueExpression.Literal(primaryKey),
                ValueExpression.Literal(fieldName),
                value
            });
        }
    }
}
