namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed mutations for <see cref="FusionRichTextEditor"/> beyond Value/SetValue.
    /// Typed Value&lt;TProp&gt;() and SetValue&lt;TProp&gt;(...) are provided by the
    /// <see cref="InputComponentRef{TComponent, TModel}"/> base class.
    /// </summary>
    public static class FusionRichTextEditorExtensions
    {
        /// <summary>Moves focus into the rich text editor.</summary>
        public static ComponentRef<FusionRichTextEditor, TModel> FocusIn<TModel>(
            this ComponentRef<FusionRichTextEditor, TModel> self)
            where TModel : class
            => self.EmitCall("focusIn");
    }
}
