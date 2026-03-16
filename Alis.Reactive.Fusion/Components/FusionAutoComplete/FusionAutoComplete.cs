namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion AutoComplete component (text input with filtering/autocomplete).
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionAutoComplete&gt;(m => m.Physician) to unlock
    /// the AutoComplete-specific extension methods.
    /// </summary>
    public sealed class FusionAutoComplete : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
