namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Provides button-specific mutations for reactive pipelines.
    /// </summary>
    public static class NativeButtonExtensions
    {
        /// <summary>Sets the button text content.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="text">The text content to display.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<NativeButton, TModel> SetText<TModel>(
            this ComponentRef<NativeButton, TModel> self, string text)
            where TModel : class
        {
            return self.Set("textContent", text);
        }

        /// <summary>Moves focus to the button element.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<NativeButton, TModel> FocusIn<TModel>(
            this ComponentRef<NativeButton, TModel> self)
            where TModel : class
        {
            return self.Call("focus");
        }
    }
}
