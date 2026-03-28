using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionDropDownList"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionDropDownList&gt;(m =&gt; m.Country).SetValue("US")</c>.
    /// </remarks>
    public static class FusionDropDownListExtensions
    {
        private static readonly FusionDropDownList Component = new FusionDropDownList();

        /// <summary>Sets the selected value.</summary>
        /// <param name="value">The value to select, or <see langword="null"/> to clear.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDropDownList, TModel> SetValue<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string? value)
            where TModel : class
            => self.Emit(new SetPropMutation("value"), value: value);

        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        /// <param name="text">The text to display.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDropDownList, TModel> SetText<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self, string text)
            where TModel : class
            => self.Emit(new SetPropMutation("text"), value: text);

        /// <summary>Replaces the data source with items from an event payload.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The event payload type containing the items.</typeparam>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the items collection from the payload.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDropDownList, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionDropDownList, TModel> self,
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
        public static ComponentRef<FusionDropDownList, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionDropDownList, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.Emit(new SetPropMutation("dataSource"), source: new EventSource(sourcePath));
        }

        /// <summary>Flushes pending property changes to the component in the browser.</summary>
        /// <remarks>
        /// Call after <see cref="SetDataSource{TModel,TSource}"/> or
        /// <see cref="SetDataSource{TModel,TResponse}"/> to make the new items appear.
        /// </remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDropDownList, TModel> DataBind<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("dataBind"));

        /// <summary>Moves focus into the dropdown.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDropDownList, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        /// <summary>Removes focus from the dropdown.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDropDownList, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusOut"));

        /// <summary>Opens the dropdown popup.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDropDownList, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("showPopup"));

        /// <summary>Closes the dropdown popup.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionDropDownList, TModel> HidePopup<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("hidePopup"));

        /// <summary>Reads the current selected value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the dropdown's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionDropDownList, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
