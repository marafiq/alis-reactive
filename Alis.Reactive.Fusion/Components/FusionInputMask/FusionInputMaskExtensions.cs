namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionInputMask"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionInputMaskExtensions
    {
        /// <summary>Moves focus into the masked input.</summary>
        public static ComponentRef<FusionInputMask, TModel> FocusIn<TModel>(
            this ComponentRef<FusionInputMask, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");
    }
}
