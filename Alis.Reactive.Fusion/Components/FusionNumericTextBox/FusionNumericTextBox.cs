namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion NumericTextBox component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionNumericTextBox&gt;(m => m.Amount) to unlock
    /// the NumericTextBox-specific extension methods.
    /// </summary>
    [ReadExpr("value")]
    public sealed class FusionNumericTextBox : FusionComponent, IFusionInputComponent, IReadableComponent
    {
    }
}
