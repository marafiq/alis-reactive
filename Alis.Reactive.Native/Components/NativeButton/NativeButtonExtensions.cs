using Alis.Reactive;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Vertical slice extension methods for NativeButton.
    ///
    /// Structured mutation convention (direct DOM):
    ///   Prop  → { prop: "textContent" }
    ///   Call  → { method: "focus" }
    ///   Read  → ref:{id}.textContent
    /// </summary>
    public static class NativeButtonExtensions
    {
        // ── Prop: sets the button's visible text ──

        public static ComponentRef<NativeButton, TModel> SetText<TModel>(
            this ComponentRef<NativeButton, TModel> self, string text)
            where TModel : class
        {
            return self.Emit(prop: "textContent", value: text);
        }

        // ── Call: gives the button focus ──

        public static ComponentRef<NativeButton, TModel> FocusIn<TModel>(
            this ComponentRef<NativeButton, TModel> self)
            where TModel : class
        {
            return self.Emit(method: "focus");
        }

        // ── Read: returns the button's current text ──

        /// <summary>
        /// Returns the BindExpr for reading this button's current text content.
        /// The TS resolver resolves ref:{id}.textContent to document.getElementById(id).textContent
        /// </summary>
        public static string Text<TModel>(
            this ComponentRef<NativeButton, TModel> self)
            where TModel : class
        {
            return $"ref:{self.TargetId}.textContent";
        }
    }
}
