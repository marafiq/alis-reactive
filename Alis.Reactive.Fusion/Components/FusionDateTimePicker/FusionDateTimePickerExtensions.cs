namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionDateTimePicker"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionDateTimePickerExtensions
    {
        /// <summary>Moves focus into the date-time picker.</summary>
        public static ComponentRef<FusionDateTimePicker, TModel> FocusIn<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");

        /// <summary>Removes focus from the date-time picker.</summary>
        public static ComponentRef<FusionDateTimePicker, TModel> FocusOut<TModel>(
            this ComponentRef<FusionDateTimePicker, TModel> self)
            where TModel : class
            => self.EmitCall("focusOut");
    }
}
