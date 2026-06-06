using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Writes, focuses, and reads the masked input value in a Reactive Plan pipeline.
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
        public static ComponentRef<FusionInputMask, TModel> SetValue<TModel>(
            this ComponentRef<FusionInputMask, TModel> self, string value)
            where TModel : class
        {
            return self.EmitSet(ValueProperty, ValueExpression.Literal(value));
        }

        /// <summary>Moves focus into the masked input.</summary>
        public static ComponentRef<FusionInputMask, TModel> FocusIn<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => self.EmitCall(FocusInMethod);

        /// <summary>Reads the masked value for conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument for component operations:
        /// <c>p.When(p.Component&lt;FusionInputMask&gt;(m =&gt; m.Phone).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => self.Read(ValueProperty);
    }
}
