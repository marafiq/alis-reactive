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
        private static readonly FusionAutoComplete Component = new FusionAutoComplete();

        /// <summary>Sets the selected value.</summary>
        /// <param name="value">The value to select, or <see langword="null"/> to clear.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> SetValue<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string? value)
            where TModel : class
            => self.EmitSet("value", ValueProducer.Literal(value));

        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        /// <param name="text">The text to display.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> SetText<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self, string text)
            where TModel : class
            => self.EmitSet("text", ValueProducer.Literal(text));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The event payload type containing the items.</typeparam>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the items collection from the payload.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet("dataSource", ValueProducer.Read(PayloadSource.Event(), sourcePath));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TResponse">The response body type containing the items.</typeparam>
        /// <param name="source">The response body instance.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionAutoComplete, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitSet("dataSource", ValueProducer.Read(source.Scope, sourcePath));
        }

        /// <summary>
        /// Flushes pending property changes to the component in the browser.
        /// </summary>
        /// <remarks>
        /// Required after <c>SetDataSource</c> in cascade patterns (Changed event).
        /// Not needed when using <c>updateData()</c> in filtering patterns.
        /// </remarks>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> DataBind<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("dataBind");

        /// <summary>Moves focus into the autocomplete input.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> FocusIn<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");

        /// <summary>Removes focus from the autocomplete input.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> FocusOut<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("focusOut");

        /// <summary>Opens the suggestion popup.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("showPopup");

        /// <summary>Closes the suggestion popup.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> HidePopup<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitCall("hidePopup");

        // NOTE: showSpinner/hideSpinner have no visible effect on SF AutoComplete.
        // refresh() causes focus loss mid-typing, not usable during filtering.
        // Both verified manually. Omitted intentionally.

        /// <summary>Enables the autocomplete input for user interaction.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> Enable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitSet("enabled", ValueProducer.Literal(true));

        /// <summary>Disables the autocomplete input, preventing user interaction.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionAutoComplete, TModel> Disable<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            => self.EmitSet("enabled", ValueProducer.Literal(false));

        /// <summary>Reads the current selected value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the autocomplete's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionAutoComplete, TModel> self)
            where TModel : class
            { self.Pipeline.Context.EnsureComponent(self.TargetId, Component.Vendor); self.Pipeline.Context.EnsureProperty(self.TargetId, Component.ValueMember, Component.ValueMember, Shape.String, "read"); return new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ValueMember); }
    }
}
