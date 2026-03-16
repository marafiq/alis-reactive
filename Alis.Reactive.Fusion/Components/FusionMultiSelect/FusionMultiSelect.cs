namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion MultiSelect component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionMultiSelect&gt;(m => m.Allergies) to unlock
    /// the MultiSelect-specific extension methods.
    /// </summary>
    public sealed class FusionMultiSelect : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
