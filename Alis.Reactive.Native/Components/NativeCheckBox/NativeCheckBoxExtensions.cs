using Alis.Reactive;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Vertical slice extension methods for NativeCheckBox.
    ///
    /// Native jsEmit convention (direct DOM):
    ///   Prop  → el.checked=val
    ///   Call  → el.focus()
    ///   Read  → ref:{id}.checked
    /// </summary>
    public static class NativeCheckBoxExtensions
    {
        public static ComponentRef<NativeCheckBox, TModel> SetChecked<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self, bool isChecked)
            where TModel : class
        {
            return self.Emit("el.checked=(val==='true')", isChecked ? "true" : "false");
        }

        public static ComponentRef<NativeCheckBox, TModel> FocusIn<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return self.Emit("el.focus()");
        }

        /// <summary>
        /// Returns the BindExpr for reading this checkbox's current checked state.
        /// </summary>
        public static string Checked<TModel>(
            this ComponentRef<NativeCheckBox, TModel> self)
            where TModel : class
        {
            return $"ref:{self.TargetId}.checked";
        }
    }
}
