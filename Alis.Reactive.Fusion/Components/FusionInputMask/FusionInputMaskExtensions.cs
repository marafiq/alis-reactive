using Alis.Reactive.Builders.Conditions;

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

        /// <summary>Sets the masked input value.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInputMask, TModel> SetValue<TModel>(
            this ComponentRef<FusionInputMask, TModel> self, string value)
            where TModel : class
        {
            return self.Set("value", value);
        }

        /// <summary>Moves focus into the masked input.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInputMask, TModel> FocusIn<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => self.Call("focusIn");

        /// <summary>Reads the current masked value for use in conditions or gather.</summary>
        /// <remarks>
        /// Pass to a <c>When()</c> condition guard or use as a source argument in component mutations:
        /// <c>p.When(p.Component&lt;FusionInputMask&gt;(m =&gt; m.Phone).Value()).NotNull().Then(p =&gt; { ... })</c>.
        /// </remarks>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>A typed value expression representing the masked input's current value.</returns>
        public static ComponentValueExpression<string> Value<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => new ComponentValueExpression<string>(self.TargetId, Component.Vendor, Component.ValueMemberPath);
    }
}
