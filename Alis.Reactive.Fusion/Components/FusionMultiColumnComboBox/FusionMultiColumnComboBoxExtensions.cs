using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionMultiColumnComboBox (SetValue, SetDataSource, DataBind, FocusIn, etc.).
    /// </summary>
    public static class FusionMultiColumnComboBoxExtensions
    {
        private static readonly FusionMultiColumnComboBox Component = new FusionMultiColumnComboBox();

        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetText<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self, string text)
            where TModel : class
            => self.Emit(new SetPropMutation("text"), value: text);

        // ── SetDataSource (event payload) ──

        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        // ── SetDataSource (response body) ──

        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        // ── DataBind (trigger binding after source change) ──

        public static ComponentRef<FusionMultiColumnComboBox, TModel> DataBind<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("dataBind"));

        public static ComponentRef<FusionMultiColumnComboBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        public static ComponentRef<FusionMultiColumnComboBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        public static ComponentRef<FusionMultiColumnComboBox, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        public static ComponentRef<FusionMultiColumnComboBox, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
