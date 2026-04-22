namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Pipeline extensions for <see cref="NativeCheckList"/>: focus-in.
    /// Typed Value&lt;TProp&gt;() and all SetValue&lt;TProp&gt;(...) overloads
    /// (literal, typed-source, response-body, event-payload) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class NativeCheckListExtensions
    {
        /// <summary>
        /// Moves keyboard focus into the check list.
        /// </summary>
        public static ComponentRef<NativeCheckList, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckList, TModel> self)
            where TModel : class
            => self.EmitCall("focus");
    }
}
