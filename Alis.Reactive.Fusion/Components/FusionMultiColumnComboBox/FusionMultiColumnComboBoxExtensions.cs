using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionMultiColumnComboBox"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionMultiColumnComboBox&gt;(m =&gt; m.Facility).SetValue("FAC-001")</c>.
    /// </remarks>
    public static class FusionMultiColumnComboBoxExtensions
    {
        private static readonly FusionMultiColumnComboBox Component = new FusionMultiColumnComboBox();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentProperty<string> TextProperty =
            ComponentProperty<string>.Named("text");

        private static readonly ComponentProperty<object> DataSourceProperty =
            ComponentProperty<object>.Named("dataSource");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        private static readonly ComponentMethod FocusOutMethod =
            ComponentMethod.Named("focusOut");

        private static readonly ComponentMethod ShowPopupMethod =
            ComponentMethod.Named("showPopup");

        private static readonly ComponentMethod HidePopupMethod =
            ComponentMethod.Named("hidePopup");

        /// <summary>Sets the selected value.</summary>
        /// <param name="value">The value to select, or <see langword="null"/> to clear.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self, string? value)
            where TModel : class
            => self.EmitSet(ValueProperty, ValueProducer.LiteralRaw(value, Shape.String));

        /// <summary>Sets the displayed text without changing the underlying value.</summary>
        /// <param name="text">The text to display.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetText<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self, string text)
            where TModel : class
            => self.EmitSet(TextProperty, ValueProducer.Literal(text));

        /// <summary>Replaces the data source with items from an event payload.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TSource">The event payload type containing the items.</typeparam>
        /// <param name="source">The event payload instance.</param>
        /// <param name="path">Expression selecting the items collection from the payload.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TSource>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
            TSource source, Expression<Func<TSource, object?>> path)
            where TModel : class
        {
            var sourcePath = ExpressionPathHelper.ToEventPath(path);
            return self.EmitSet(DataSourceProperty, ValueProducer.Read(PayloadSource.Event(), sourcePath));
        }

        /// <summary>Replaces the data source with items from an HTTP response body.</summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TResponse">The response body type containing the items.</typeparam>
        /// <param name="source">The response body instance.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            return self.EmitSet(DataSourceProperty, ValueProducer.Read(source.Scope, sourcePath));
        }

        /// <summary>Flushes pending property changes to the component in the browser.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> DataBind<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(DataBindMethod);

        /// <summary>Moves focus into the combo box.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Removes focus from the combo box.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> FocusOut<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(FocusOutMethod);

        /// <summary>Opens the multi-column dropdown popup.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> ShowPopup<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(ShowPopupMethod);

        /// <summary>Closes the multi-column dropdown popup.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionMultiColumnComboBox, TModel> HidePopup<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.EmitCall(HidePopupMethod);

        /// <summary>Reads the current selected value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionMultiColumnComboBox&gt;(m =&gt; m.Facility).Value()).Eq("FAC-001").Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the combo box's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionMultiColumnComboBox, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
