using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Runtime behavior for Syncfusion PivotView. Initial report configuration stays on
    /// Syncfusion's PivotViewBuilder.
    /// </summary>
    public static class FusionPivotViewExtensions
    {
        private static readonly ComponentProperty<string> CurrentViewProperty =
            ComponentProperty<string>.Named("currentView");

        private static readonly ComponentProperty<object> DataSourceProperty =
            ComponentProperty<object>.Mapped("dataSource", "dataSourceSettings.dataSource");

        private static readonly ComponentMethod RefreshMethod =
            ComponentMethod.Named("refresh");

        private static readonly ComponentMethod GetPersistDataMethod =
            ComponentMethod.Named("getPersistData").WithArgs<bool>();

        private static readonly ComponentMethod LoadPersistDataMethod =
            ComponentMethod.Named("loadPersistData").WithArgs<string>();

        private static readonly ComponentMethod ConditionalFormattingDialogMethod =
            ComponentMethod.Named("showConditionalFormattingDialog");

        private static readonly ComponentMethod CalculatedFieldDialogMethod =
            ComponentMethod.Named("createCalculatedFieldDialog");

        public static TypedComponentSource<string> CurrentView<TModel>(
            this ComponentRef<FusionPivotView, TModel> self)
            where TModel : class
            => self.Read(CurrentViewProperty);

        public static TypedComponentSource<string> PersistedLayout<TModel>(
            this ComponentRef<FusionPivotView, TModel> self,
            bool removeDataSource = true)
            where TModel : class
            => self.Read<string>(
                GetPersistDataMethod,
                new List<ValueExpression> { ValueExpression.Literal(removeDataSource) });

        public static ComponentRef<FusionPivotView, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionPivotView, TModel> self,
            ResponseBody<TResponse> source,
            Expression<System.Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            self.EmitSet(DataSourceProperty, ValueExpression.Read(source.Scope, sourcePath));
            return self.Refresh();
        }

        public static ComponentRef<FusionPivotView, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionPivotView, TModel> self,
            ResponseBody<TResponse> source)
            where TModel : class
            where TResponse : class
        {
            self.EmitSet(DataSourceProperty, ValueExpression.Read(source.Scope, "responseBody"));
            return self.Refresh();
        }

        public static ComponentRef<FusionPivotView, TModel> LoadPersistedLayout<TModel>(
            this ComponentRef<FusionPivotView, TModel> self,
            string persistedLayout)
            where TModel : class
            => self.EmitCall(
                LoadPersistDataMethod,
                new List<ValueExpression> { ValueExpression.Literal(persistedLayout) });

        public static ComponentRef<FusionPivotView, TModel> LoadPersistedLayout<TModel, TResponse>(
            this ComponentRef<FusionPivotView, TModel> self,
            ResponseBody<TResponse> source,
            Expression<System.Func<TResponse, string>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitCall(
                LoadPersistDataMethod,
                new List<ValueExpression>
                {
                    ValueExpression.Read(source.Scope, sourcePath, Shape.String)
                });
        }

        public static ComponentRef<FusionPivotView, TModel> Refresh<TModel>(
            this ComponentRef<FusionPivotView, TModel> self)
            where TModel : class
            => self.EmitCall(RefreshMethod);

        public static ComponentRef<FusionPivotView, TModel> ShowConditionalFormattingDialog<TModel>(
            this ComponentRef<FusionPivotView, TModel> self)
            where TModel : class
            => self.EmitCall(ConditionalFormattingDialogMethod);

        public static ComponentRef<FusionPivotView, TModel> CreateCalculatedFieldDialog<TModel>(
            this ComponentRef<FusionPivotView, TModel> self)
            where TModel : class
            => self.EmitCall(CalculatedFieldDialogMethod);
    }
}
