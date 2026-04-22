namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionTimePicker"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionTimePickerExtensions
    {
        /// <summary>Moves focus into the time picker.</summary>
        public static ComponentRef<FusionTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");

        /// <summary>Removes focus from the time picker.</summary>
        public static ComponentRef<FusionTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionTimePicker, TModel> self)
            where TModel : class
            => self.EmitCall("focusOut");
    }
}
