namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeTextBox"/>: focus-in.
    /// </summary>
    /// <remarks>
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(value) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class and work uniformly
    /// across all input components.
    /// </remarks>
    public static class NativeTextBoxExtensions
    {
        /// <summary>
        /// Moves keyboard focus into the text input.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextBox, TModel> self)
            where TModel : class
        {
            return self.EmitCall("focus");
        }
    }
}
