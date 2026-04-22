namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeDropDown"/>: focus-in.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(value) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class NativeDropDownExtensions
    {
        /// <summary>Moves keyboard focus into the dropdown.</summary>
        public static ComponentRef<NativeDropDown, TModel> FocusIn<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
            => self.EmitCall("focus");
    }
}
