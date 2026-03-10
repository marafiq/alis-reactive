using System.Globalization;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Vertical slice extension methods for FusionNumericTextBox.
    ///
    /// Fusion jsEmit convention (el.ej2_instances[0]):
    ///   Prop  → var c=el.ej2_instances[0]; c.value=Number(val); c.dataBind()
    ///   Call  → el.ej2_instances[0].focusIn()
    ///   Read  → ref:{id}.value
    /// </summary>
    public static class FusionNumericTextBoxExtensions
    {
        // ── Prop: assigns value, coerces to Number, calls dataBind() ──

        public static ComponentRef<FusionNumericTextBox, TModel> SetValue<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self, decimal value)
            where TModel : class
        {
            return self.Emit(
                "var c=el.ej2_instances[0]; c.value=Number(val); c.dataBind()",
                value.ToString(CultureInfo.InvariantCulture));
        }

        // ── Call: invokes focusIn() on the SF instance ──

        public static ComponentRef<FusionNumericTextBox, TModel> FocusIn<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return self.Emit("el.ej2_instances[0].focusIn()");
        }

        // ── Read: returns BindExpr for the component's current value ──

        /// <summary>
        /// Returns the BindExpr for reading this component's current numeric value.
        /// The TS resolver resolves comp.value via evalRead (vendor-aware)
        /// </summary>
        public static string Value<TModel>(
            this ComponentRef<FusionNumericTextBox, TModel> self)
            where TModel : class
        {
            return $"ref:{self.TargetId}.value";
        }
    }
}
