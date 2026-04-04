namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Provides test-widget mutations and reads used by the architecture verification surface.
    /// </summary>
    public static class TestWidgetNativeExtensions
    {
        /// <summary>Sets the widget value.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetNative, TModel> SetValue<TModel>(
            this ComponentRef<TestWidgetNative, TModel> self, string value)
            where TModel : class => self.Set(TestWidgetNative.Value, value);

        /// <summary>Moves focus to the widget input.</summary>
        /// <param name="self">The component reference to operate on.</param>
        /// <returns>The component reference for continued chaining.</returns>
        public static ComponentRef<TestWidgetNative, TModel> Focus<TModel>(
            this ComponentRef<TestWidgetNative, TModel> self)
            where TModel : class => self.Call(TestWidgetNative.Focus);
    }
}
