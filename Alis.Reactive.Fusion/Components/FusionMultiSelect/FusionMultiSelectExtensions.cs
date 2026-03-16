using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for FusionMultiSelect (SetValue, SetDataSource, DataBind, Value).
    /// </summary>
    public static class FusionMultiSelectExtensions
    {
        private static readonly FusionMultiSelect Component = new FusionMultiSelect();

        public static ComponentRef<FusionMultiSelect, TModel> SetValue<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        // ── SetDataSource (event payload) ──

        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        // ── SetDataSource (response body) ──

        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        // ── DataBind (trigger binding after source change) ──

        public static ComponentRef<FusionMultiSelect, TModel> DataBind<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("dataBind"));

        public static ComponentRef<FusionMultiSelect, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        public static ComponentRef<FusionMultiSelect, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
