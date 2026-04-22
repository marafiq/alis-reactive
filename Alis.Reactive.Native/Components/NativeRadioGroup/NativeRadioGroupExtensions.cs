namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeRadioGroup"/>: focus-in.
    /// Typed Value&lt;TProp&gt;() and all SetValue&lt;TProp&gt;(...) overloads
    /// (literal, typed-source, response-body, event-payload) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class NativeRadioGroupExtensions
    {
        /// <summary>Moves keyboard focus into the radio group.</summary>
        public static ComponentRef<NativeRadioGroup, TModel> FocusIn<TModel>(
            this ComponentRef<NativeRadioGroup, TModel> self)
            where TModel : class
            => self.EmitCall("focus");
    }
}
