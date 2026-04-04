using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;

using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Provides member access helpers for the Syncfusion test widget.
    /// </summary>
    public static class TestWidgetSyncFusionExtensions
    {
        private static readonly CapabilityMethod FocusMethod = CapabilityMethod.Named("focus");
        private static readonly CapabilityMethod ClearMethod = CapabilityMethod.Named("clear");
        private static readonly CapabilityMethod SetItemsMethod = CapabilityMethod.Named("setItems");

        /// <summary>Sets the widget value from a literal string.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, string value)
            where TModel : class => self.Set(TestWidgetSyncFusion.Value, value);

        /// <summary>Sets the widget value from an event payload path.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The event payload placeholder.</param>
        /// <param name="path">The event path selecting the value.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TSource>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
            => self.SetFromEvent(TestWidgetSyncFusion.Value, path);

        /// <summary>Sets the widget value from an HTTP response path.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The response body placeholder.</param>
        /// <param name="path">The response path selecting the value.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TResponse>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
            => self.SetFromResponse(TestWidgetSyncFusion.Value, path);

        /// <summary>Sets the widget value from another typed value expression.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TProp">The source value type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The source to copy from.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel, TProp>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, ReactiveValue<TProp> source)
            where TModel : class
            => self.Set(TestWidgetSyncFusion.Value, source);

        /// <summary>Reads the current widget value for conditions and gathers.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the widget value.</returns>
        public static ReactiveValue<string> Value<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.CreateValue<string>();

        /// <summary>Calls the widget <c>focus</c> method.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetSyncFusion, TModel> Focus<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Call(FocusMethod);

        /// <summary>Calls the widget <c>clear</c> method.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetSyncFusion, TModel> Clear<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Call(ClearMethod);

        /// <summary>Calls <c>setItems</c> with items read from an event payload path.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TSource">The event payload type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The event payload placeholder.</param>
        /// <param name="path">The event path selecting the items argument.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel, TSource>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
            => self.CallFromEvent(SetItemsMethod, path);

        /// <summary>Calls <c>setItems</c> with items read from an HTTP response path.</summary>
        /// <typeparam name="TModel">The page model type that owns the reactive plan.</typeparam>
        /// <typeparam name="TResponse">The response body type.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The response body placeholder.</param>
        /// <param name="path">The response path selecting the items argument.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel, TResponse>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
            => self.CallFromResponse(SetItemsMethod, path);
    }
}
