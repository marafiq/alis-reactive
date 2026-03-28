using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeTextBox"/>: set value, focus, and read.
    /// </summary>
    public static class NativeTextBoxExtensions
    {
        private static readonly NativeTextBox _component = new NativeTextBox();

        /// <summary>
        /// Sets the text input value in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="self">The text box component reference.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeTextBox, TModel> SetValue<TModel>(
            this ComponentRef<NativeTextBox, TModel> self, string value)
            where TModel : class
        {
            return self.Emit(new SetPropMutation("value"), value: value);
        }

        /// <summary>
        /// Moves keyboard focus into the text input.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextBox, TModel> self)
            where TModel : class
        {
            return self.Emit(new CallMutation("focus"));
        }

        /// <summary>
        /// Reads the current text input value for use in conditions or gather.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>A typed source representing the input's current value.</returns>
        public static TypedComponentSource<string> Value<TModel>(
            this ComponentRef<NativeTextBox, TModel> self)
            where TModel : class
        {
            return new TypedComponentSource<string>(self.TargetId, _component.Vendor, _component.ReadExpr);
        }
    }
}
