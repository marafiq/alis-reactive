using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;

using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionAutoComplete"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician).SetValue("Dr. Smith")</c>.
    /// </remarks>
    public static class FusionAutoCompleteExtensions
    {
        private static readonly CapabilityProperty TextProperty = CapabilityProperty.Named("text");
        private static readonly CapabilityProperty DataSourceProperty = CapabilityProperty.Named("dataSource");
        private static readonly CapabilityProperty EnabledProperty = CapabilityProperty.Named("enabled");
        private static readonly CapabilityMethod DataBindMethod = CapabilityMethod.Named("dataBind");
        private static readonly CapabilityMethod FocusInMethod = CapabilityMethod.Named("focusIn");
        private static readonly CapabilityMethod FocusOutMethod = CapabilityMethod.Named("focusOut");
        private static readonly CapabilityMethod ShowPopupMethod = CapabilityMethod.Named("showPopup");
        private static readonly CapabilityMethod HidePopupMethod = CapabilityMethod.Named("hidePopup");

        /// <summary>Sets the selected value.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The value to select, or <see langword="null"/> to clear.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> SetValue<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string? value)
            where TModel : class
            => self.Set(FusionAutoComplete.Value, value);

        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="text">The text to display.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> SetText<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string text)
            where TModel : class
            => self.Set(TextProperty, text);

        /// <summary>Replaces the data source with items from an event payload.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The event payload type containing the items.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the items collection from the payload.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
            => self.SetFromEvent(DataSourceProperty, path);

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TResponse">The response body type containing the items.</typeparam>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="source">The response body instance.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
            => self.SetFromResponse(DataSourceProperty, path);

        /// <summary>
        /// Flushes pending property changes to the component in the browser.
        /// </summary>
        /// <remarks>
        /// Required after <c>SetDataSource</c> in cascade patterns (Changed event).
        /// Not needed when using <c>updateData()</c> in filtering patterns.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> DataBind<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Call(DataBindMethod);

        /// <summary>Moves focus into the autocomplete input.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> FocusIn<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Call(FocusInMethod);

        /// <summary>Removes focus from the autocomplete input.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> FocusOut<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Call(FocusOutMethod);

        /// <summary>Opens the suggestion popup.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Call(ShowPopupMethod);

        /// <summary>Closes the suggestion popup.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> HidePopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Call(HidePopupMethod);

        // NOTE: showSpinner/hideSpinner have no visible effect on SF AutoComplete.
        // refresh() causes focus loss mid-typing, not usable during filtering.
        // Both verified manually. Omitted intentionally.

        /// <summary>Enables the autocomplete input for user interaction.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> Enable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Set(EnabledProperty, true);

        /// <summary>Disables the autocomplete input, preventing user interaction.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> Disable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.Set(EnabledProperty, false);

        /// <summary>Reads the current selected value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the autocomplete's current value.</returns>
        public static ReactiveValue<string> Value<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.CreateValue<string>();
    }
}
