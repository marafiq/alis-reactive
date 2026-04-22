namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeTextArea"/>: focus-in.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(value) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class NativeTextAreaExtensions
    {
        /// <summary>
        /// Moves keyboard focus into the textarea.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <returns>The component reference for method chaining.</returns>
        public static ComponentRef<NativeTextArea, TModel> FocusIn<TModel>(
            this ComponentRef<NativeTextArea, TModel> self)
            where TModel : class
        {
            return self.EmitCall("focus");
        }
    }
}
