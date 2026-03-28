using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations and value reading for <see cref="FusionInputMask"/> in a reactive pipeline.
    /// </summary>
    public static class FusionInputMaskExtensions
    {
        private static readonly FusionInputMask Component = new FusionInputMask();

        /// <summary>Sets the masked input value.</summary>
        /// <param name="value">The value to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInputMask, TModel> SetValue<TModel>(
            this ComponentRef<FusionInputMask, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        /// <summary>Moves focus into the masked input.</summary>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<FusionInputMask, TModel> FocusIn<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => self.Emit(new CallMutation("focusIn"));

        /// <summary>Reads the current masked value for use in conditions or gather.</summary>
        /// <returns>A typed source representing the masked input's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => new TypedComponentSource<string>(self.TargetId, Component.Vendor, Component.ReadExpr);
    }
}
