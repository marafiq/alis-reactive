using Alis.Reactive;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Vertical slice extension methods for NativeDropDown.
    ///
    /// Native jsEmit convention (direct DOM — no ej2_instances, no dataBind):
    ///   Prop  → el.value=val
    ///   Call  → el.focus()
    ///   Read  → ref:{id}.value
    /// </summary>
    public static class NativeDropDownExtensions
    {
        // ── Prop: assigns the select element's value ──

        public static ComponentRef<NativeDropDown, TModel> SetValue<TModel>(
            this ComponentRef<NativeDropDown, TModel> self, string value)
            where TModel : class
        {
            return self.Emit("el.value=val", value);
        }

        // ── Call: gives the select element focus ──

        public static ComponentRef<NativeDropDown, TModel> FocusIn<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
        {
            return self.Emit("el.focus()");
        }

        // ── Read: returns the select element's current value ──

        /// <summary>
        /// Returns the BindExpr for reading this dropdown's current selected value.
        /// The TS resolver resolves ref:{id}.value to document.getElementById(id).value
        /// </summary>
        public static string Value<TModel>(
            this ComponentRef<NativeDropDown, TModel> self)
            where TModel : class
        {
            return $"ref:{self.TargetId}.value";
        }
    }
}
