using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionMultiSelect"/> in a reactive pipeline.
    /// </summary>
    public static class FusionMultiSelectExtensions
    {
        private static readonly FusionMultiSelect Component = new FusionMultiSelect();

        /// <summary>Sets the selected values.</summary>
        /// <param name="value">The values to select, or <see langword="null"/> to clear.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiSelect, TModel> SetValue<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self, string[]? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        /// <summary>Replaces the data source with items from an event payload.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The event payload type containing the items.</typeparam>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the items collection from the payload.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TResponse">The response body type containing the items.</typeparam>
        /// <param name="source">The response body instance.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiSelect, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionMultiSelect, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        /// <summary>Flushes pending property changes to the component in the browser.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiSelect, TModel> DataBind<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("dataBind"));

        /// <summary>Opens the selection popup.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiSelect, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        /// <summary>Closes the selection popup.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiSelect, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        /// <summary>Reads the current selected values for use in conditions or gather.</summary>
        /// <returns>A typed source representing the multi-select's current values.</returns>
        public static TypedComponentSource<string[]> Value<TModel>(
            this ComponentRef<FusionMultiSelect, TModel> self)
            where TModel : class
            => new TypedComponentSource<string[]>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
