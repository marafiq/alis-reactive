using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionInputMask"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionInputMask&gt;(m =&gt; m.Phone).SetValue("555-0123")</c>.
    /// </remarks>
    public static class FusionInputMaskExtensions
    {
        private static readonly FusionInputMask Component = new FusionInputMask();

        private static readonly ComponentProperty<string> ValueProperty =
            ComponentProperty<string>.Named(Component.ValueMember);

        private static readonly ComponentMethod FocusInMethod =
            ComponentMethod.Named("focusIn");

        /// <summary>Sets the masked input value.</summary>
        /// <param name="value">The value to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInputMask, TModel> SetValue<TModel>(
            this ComponentRef<FusionInputMask, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueProducer.Literal(value));
        }

        /// <summary>Moves focus into the masked input.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInputMask, TModel> FocusIn<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Reads the current masked value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionInputMask&gt;(m =&gt; m.Phone).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <returns>A typed source representing the masked input's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
